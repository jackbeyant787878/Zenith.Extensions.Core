using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zenith.RabbitMQ.Core;

/// <summary>
/// Manages a single centralized physical TCP connection asynchronously using RabbitMQ.Client.
/// </summary>
public class RabbitMQPersistentConnection : IAsyncDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQPersistentConnection> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQPersistentConnection> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates if the current physical connection is alive and open.
    /// </summary>
    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    /// <summary>
    /// Spawns a modern asynchronous IChannel worker from the active connection.
    /// </summary>
    public async Task<IChannel> CreateChannelAsync(CreateChannelOptions channelOptions, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await TryConnectAsync(cancellationToken);
        }

        if (_connection == null)
            throw new InvalidOperationException("RabbitMQ connection could not be established. Cannot create channel.");

        // Pure async channel instantiation
        return await _connection.CreateChannelAsync(cancellationToken: cancellationToken,options:channelOptions);
    }

    /// <summary>
    /// Thread-safe non-blocking evaluation loop to establish the physical broker link.
    /// </summary>
    public async Task<bool> TryConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return true;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected) return true;

            _logger.LogInformation("RabbitMQ Client is establishing an asynchronous connection link...");

            // Pure async connection establishment
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            if (IsConnected)
            {
                // Hook into async event handlers required by v7+
                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

                _logger.LogInformation("RabbitMQ Client successfully bound to host cluster endpoint: '{HostName}'", _connection.Endpoint.HostName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "RabbitMQ connection subsystem encountered a critical async instantiation failure.");
            return false;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("RabbitMQ physical connection dropped. ReplyCode: {Code}, Reason: {Reason}", e.ReplyCode, e.ReplyText);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An error occurred while cleaning up the RabbitMQ connection infrastructure.");
        }
        finally
        {
            _connectionLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}