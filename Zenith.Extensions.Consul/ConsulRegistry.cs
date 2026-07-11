using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class ConsulRegistryExtensions
{
    /// <summary>
    /// Register Consul service discovery core components into DI container
    /// </summary>
    /// <param name="services">DI service collection</param>
    /// <param name="configuration">App configuration instance</param>
    /// <returns>Original service collection for chained calls</returns>
    public static IServiceCollection AddConsulRegistry(this IServiceCollection services, IConfiguration configuration)
    {
        var registryAddress = configuration["ConsulConfig:RegistryAddress"];

        if (string.IsNullOrEmpty(registryAddress))
        {
            return services;
        }

        // Register singleton Consul client with target registry address
        services.AddSingleton<IConsulClient>(sp => new ConsulClient(config =>
        {
            config.Address = new Uri(registryAddress);
        }));

        // Register background service for automatic service registration & TTL heartbeat reporting
        // Uses modern BackgroundService instead of legacy IHostedService
        services.AddHostedService<ConsulRegisterBackgroundService>();
        return services;
    }

    /// <summary>
    /// Standard background daemon based on .NET 6+ BackgroundService & PeriodicTimer
    /// Responsible for service registration, periodic TTL heartbeat and graceful deregistration
    /// </summary>
    internal class ConsulRegisterBackgroundService : BackgroundService
    {
        private readonly IConsulClient _consulClient;
        private readonly IConfiguration _configuration;
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<ConsulRegisterBackgroundService> _logger;
        private string? _serviceId;

        public ConsulRegisterBackgroundService(
            IConsulClient consulClient,
            IConfiguration configuration,
            HealthCheckService healthCheckService,
            ILogger<ConsulRegisterBackgroundService> logger)
        {
            _consulClient = consulClient;
            _configuration = configuration;
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Core execution entry of BackgroundService, runs in isolated background thread
        /// Handles initial service registration and starts periodic health heartbeat loop
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serviceName = _configuration["ConsulConfig:ServiceName"] ?? "serviceA";
            // Unique service ID to distinguish multiple instances on different machines
            _serviceId = $"{serviceName}-{Environment.MachineName}";

            // Priority logic for resolving service IP: K8s Pod IP env var > configured IP > fallback 127.0.0.1
            var serviceIp = Environment.GetEnvironmentVariable("POD_IP")
                            ?? _configuration["ConsulConfig:ServiceIP"]
                            ?? "127.0.0.1";

            var servicePort = int.Parse(_configuration["ConsulConfig:ServicePort"] ?? "5008");

            var registration = new AgentServiceRegistration()
            {
                ID = _serviceId,
                Name = serviceName,
                Address = serviceIp,
                Port = servicePort,
                // TTL health check configuration
                Check = new AgentServiceCheck()
                {
                    TTL = TimeSpan.FromSeconds(15), // Consul marks service critical if no heartbeat within 15s
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1) // Auto clean up offline service after 1 minute critical state
                },
                Tags = new[] { "anonymous_allowed" }
            };

            _logger.LogInformation("[Production] Registering service node to Consul: {ServiceId} -> {Ip}:{Port}", _serviceId, serviceIp, servicePort);
            await _consulClient.Agent.ServiceRegister(registration, stoppingToken);

            // .NET 6+ modern async timer: avoids overlapping heartbeat executions, natively supports cancellation token
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            try
            {
                // Trigger health report every 5s until host shutdown signal received
                while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExecuteHealthCheckAndReportAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Heartbeat daemon received shutdown signal, exiting loop gracefully...");
            }
        }

        /// <summary>
        /// Run global app health checks and report health status to Consul via TTL API
        /// Send PassTTL when all checks healthy; send FailTTL with failure reasons when unhealthy
        /// </summary>
        private async Task ExecuteHealthCheckAndReportAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Reuse host lifecycle cancellation token to align with app shutdown
                var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken);

                if (healthReport.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
                {
                    // Mark service healthy and reset TTL timer on Consul
                    await _consulClient.Agent.PassTTL($"service:{_serviceId}", "All internal micro-checks passed.", cancellationToken);
                }
                else
                {
                    // Collect all unhealthy health check entries
                    var failedReasons = string.Join(", ", healthReport.Entries
                        .Where(e => e.Value.Status != Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
                        .Select(e => e.Key));

                    _logger.LogWarning("[Health Warning] Internal components unhealthy: {FailedReasons, reporting critical status to Consul", failedReasons);
                    // Notify Consul that current service instance is unavailable
                    await _consulClient.Agent.FailTTL($"service:{_serviceId}", $"Internal micro-checks failed: {failedReasons}", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during health check execution or Consul TTL heartbeat reporting");
            }
        }

        /// <summary>
        /// Graceful shutdown hook, triggered when host stops (e.g. K8s pod termination)
        /// Deregister current service instance from Consul before process exits
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_serviceId))
            {
                _logger.LogInformation("Starting graceful deregistration of Consul service node: {ServiceId}", _serviceId);
                try
                {
                    // Use stop-specific cancellation token to guarantee deregistration completes before container termination
                    await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deregister service from Consul");
                }
            }

            await base.StopAsync(cancellationToken);
        }
    }
}