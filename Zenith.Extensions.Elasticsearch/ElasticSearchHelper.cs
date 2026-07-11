using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
namespace Zenith.Extensions.Elasticsearch;
public class ElasticSearchHelper
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger? _logger;

    // Circuit breaker state variables
    private static readonly ConcurrentQueue<DateTime> _failureQueue = new();
    private static int _failureFlag = 0; // 0: Normal, 1: Circuit Opened (Tripped)
    private static DateTime _protectionTime;
    private static readonly object _locker = new();

    /// <summary>
    /// Initializes a new instance of ElasticSearchHelper, dynamically reading the URL from environment variables.
    /// </summary>
    /// <param name="logger">The non-generic logger instance.</param>
    public ElasticSearchHelper(ILogger? logger = null)
    {
        _logger = logger;

        // 1. Read dynamically from environment variables, fallback to default if not configured
        string? envUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
        if (string.IsNullOrWhiteSpace(envUrl))
        {
            envUrl = "http://localhost:9200"; 
            _logger?.LogWarning("ELASTICSEARCH_URL environment variable not found. Using default: {Url}", envUrl);
        }

        var uri = new Uri(envUrl);

        // 2. Configure options using the modern Elasticsearch 9.0+ configuration client
        var settings = new ElasticsearchClientSettings(uri)
            .RequestTimeout(TimeSpan.FromSeconds(2));

        // 3. Reuse this single Client instance globally to completely prevent Socket Exhaustion
        _client = new ElasticsearchClient(settings);
    }

    /// <summary>
    /// Asynchronously logs data to the specified Elasticsearch index.
    /// </summary>
    public async Task LogAsync<T>(string index, T data) where T : class
    {
        FailureCheck();

        try
        {
            // Elasticsearch 9.0+ strictly requires index names to be lowercase
            var response = await _client.IndexAsync(data, idx => idx.Index(index.ToLower()));

            if (!response.IsValidResponse)
            {
                // In 9.0+, IsValidResponse is the standard way to check for request success
                FailureHandler(response.ElasticsearchServerError?.Error?.Reason
                               ?? "Elasticsearch internal error");
            }
        }
        catch (Exception ex)
        {
            FailureHandler(ex.Message, ex);
        }
    }

    /// <summary>
    /// Synchronously logs data to the specified Elasticsearch index.
    /// </summary>
    public void Log<T>(string index, T data) where T : class
    {
        FailureCheck();
        try
        {
            var response = _client.Index(data, idx => idx.Index(index.ToLower()));
            if (!response.IsValidResponse)
            {
                FailureHandler(response.ElasticsearchServerError?.Error?.Reason ?? "Elasticsearch internal error");
            }
        }
        catch (Exception ex)
        {
            FailureHandler(ex.Message, ex);
        }
    }

    /// <summary>
    /// Checks if the circuit breaker is currently tripped, preventing further cascading failures.
    /// </summary>
    private void FailureCheck()
    {
        // Lock-free read check for high performance under heavy load
        if (_failureFlag == 1)
        {
            if (DateTime.Now < _protectionTime.AddSeconds(90))
            {
                _logger?.LogCritical("Elasticsearch circuit breaker is active. Rejecting incoming write request.");
                throw new Exception("Protection mode: Elasticsearch service is temporarily unavailable due to consecutive errors.");
            }

            // Automatically attempt recovery after the 90-second cooldown expires
            lock (_locker)
            {
                if (_failureFlag == 1 && DateTime.Now >= _protectionTime.AddSeconds(90))
                {
                    _failureFlag = 0;

                    // Clear the failure queue to reset counter
                    while (_failureQueue.TryDequeue(out _)) { }
                    _logger?.LogInformation("Elasticsearch cooldown period expired. Circuit breaker closed, resuming traffic.");
                }
            }
        }
    }

    /// <summary>
    /// Handles tracking failures and trips the circuit breaker if thresholds are breached.
    /// </summary>
    private void FailureHandler(string reason, Exception? ex = null)
    {
        lock (_locker)
        {
            var now = DateTime.Now;

            if (_failureQueue.Count >= 3)
            {
                if (_failureQueue.TryDequeue(out var top))
                {
                    var delta = now - top;
                    // Trip the circuit breaker if 3 failures occur within a rolling 10-second window
                    if (delta.TotalSeconds < 10)
                    {
                        _failureFlag = 1;
                        _protectionTime = now;
                        _logger?.LogError("Elasticsearch encountered consecutive failures. Tripping circuit breaker for 90 seconds. Reason: {Reason}", reason);
                    }
                }
            }

            _failureQueue.Enqueue(now);
            throw new Exception($"Failed to write to ElasticSearch. Reason: {reason}", ex);
        }
    }
}