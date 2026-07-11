using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Zenith.Extensions.RabbitMQ;
namespace Zenith.RabbitMQ.Core;

/// <summary>
/// Fully asynchronous, thread-safe message producer optimized for RabbitMQ.Client v7.2.1 topology creation and publishing.
/// </summary>
public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly RabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMQProducer> _logger;

    private IChannel? _channel;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _outstandingConfirms = new();
    private bool _disposed;

    public RabbitMQProducer(RabbitMQPersistentConnection persistentConnection, ILogger<RabbitMQProducer> logger)
    {
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) return _channel;

        await _channelLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel == null)
            {

                var channelOptions = new CreateChannelOptions(publisherConfirmationsEnabled:true,publisherConfirmationTrackingEnabled:true);

                _channel = await _persistentConnection.CreateChannelAsync(channelOptions,cancellationToken);
                              
                _channel.BasicAcksAsync += OnBasicAcksAsync;
                _channel.BasicNacksAsync += OnBasicNacksAsync;
                _channel.BasicReturnAsync += OnBasicReturnAsync;
            }
            return _channel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    #region Topology Setup Management (Queue & Exchange Declarations)

    /// <summary>
    /// Asynchronously sets up the complete broker topology layout by declaring exchanges, durable queues, and structural routing links.
    /// </summary>
    public async Task SetupTopologyAsync(string exchange, string exchangeType,string queueName, string routingKey,
        Dictionary<string, object>? queueArguments = null,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(exchangeType))
        {
            throw new ArgumentException("Exchange type must be specified.", nameof(exchangeType));
        }

        if (string.IsNullOrWhiteSpace(queueName))   
        {
            throw new ArgumentException("Queue name must be specified.", nameof(queueName));
        }

  

        IChannel channel = await GetChannelAsync(cancellationToken);

        _logger.LogInformation("Declaring broker routing topology. Exchange: {Exchange} ({Type}), Queue: {Queue}, RoutingKey: {Key}",
            exchange, exchangeType, queueName, routingKey);

        // 1. Asynchronously declare a durable, non-autodelete exchange
        await channel.ExchangeDeclareAsync( exchange: exchange, type: exchangeType, durable: true,autoDelete: false, arguments: null,cancellationToken: cancellationToken);

        // 2. Asynchronously declare a durable, concurrent-safe infrastructure queue
        // durable: true -> survives broker restarts
        // exclusive: false -> allows multiple concurrent consumers to subscribe
        // autoDelete: false -> remains active even if no consumers are listening
        await channel.QueueDeclareAsync( queue: queueName, durable: true,exclusive: false, autoDelete: false, arguments: queueArguments);

        // 3. Asynchronously bind the durable queue onto the matching target exchange structure
        await channel.QueueBindAsync(queue: queueName, exchange: exchange, routingKey: routingKey ?? string.Empty,arguments: null,cancellationToken: cancellationToken);

        _logger.LogInformation("Broker topology mappings successfully established and initialized on cluster.");
    }

    #endregion

    #region Publishing Engine Core

    public Task<bool> PublishAsync<T>(string exchange, string routingKey, T message, PublishOptions? options = null)
        => CorePublishEngineAsync(exchange, routingKey, ExchangeType.Direct, message, options);

    public Task<bool> PublishFanoutAsync<T>(string exchange, T message, PublishOptions? options = null)
        => CorePublishEngineAsync(exchange, string.Empty, ExchangeType.Fanout, message, options);

    public Task<bool> PublishTopicAsync<T>(string exchange, string routingKey, T message, PublishOptions? options = null)
        => CorePublishEngineAsync(exchange, routingKey, ExchangeType.Topic, message, options);

    public Task<bool> PublishHeadersAsync<T>(string exchange, Dictionary<string, object> headers, T message, PublishOptions? options = null)
    {
        options ??= new PublishOptions { Headers = headers };
        return CorePublishEngineAsync(exchange, string.Empty, ExchangeType.Headers, message, options);
    }

    private async Task<bool> CorePublishEngineAsync<T>(string exchange, string routingKey, string exchangeType, T message, PublishOptions? options)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = cts.Token;

        IChannel channel = await GetChannelAsync(token);

        // Optional dynamic setup of exchange (Fallback to guarantee structural stability)
        Dictionary<string, object>? exchangeArgs = null;
        if (!string.IsNullOrEmpty(options?.AlternateExchange))
        {
            exchangeArgs = new Dictionary<string, object> { { "alternate-exchange", options.AlternateExchange } };
        }

        await channel.ExchangeDeclareAsync(exchange: exchange,type: exchangeType,durable: true, autoDelete: false, arguments: exchangeArgs,cancellationToken: token);

        string jsonPayload = message is string str ? str : JsonSerializer.Serialize(message);
        byte[] body = Encoding.UTF8.GetBytes(jsonPayload);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent // Replaces properties.Persistent = true in v7
        };

        if (options?.MessageTtl.HasValue == true)
        {
            properties.Expiration = options.MessageTtl.Value.ToString();
        }
        if (options?.Headers != null && exchangeType == ExchangeType.Headers)
        {
            properties.Headers = options.Headers;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync();
        _outstandingConfirms.TryAdd(sequenceNumber, tcs);

        try
        {
            await channel.BasicPublishAsync(exchange: exchange, routingKey: routingKey, mandatory: true,basicProperties: properties, body: body,cancellationToken: token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Socket writing failure encountered. Evicting sequence tracking ID: {Seq}", sequenceNumber);
            _outstandingConfirms.TryRemove(sequenceNumber, out _);
            throw;
        }

        using (token.Register(() => tcs.TrySetException(new TimeoutException($"RabbitMQ Server Broker Ack Timeout for Sequence Tracking ID: {sequenceNumber}"))))
        {
            return await tcs.Task;
        }
    }

    #endregion

    #region Asynchronous Publisher Confirmation Handlers (v7 Async Hooks)

    private Task OnBasicAcksAsync(object sender, BasicAckEventArgs e)
    {
        HandleConfirmation(e.DeliveryTag, e.Multiple, isSuccess: true);
        return Task.CompletedTask;
    }

    private Task OnBasicNacksAsync(object sender, BasicNackEventArgs e)
    {
        _logger.LogWarning("RabbitMQ Broker negatively acknowledged message delivery. DeliveryTag: {Tag}", e.DeliveryTag);
        HandleConfirmation(e.DeliveryTag, e.Multiple, isSuccess: false);
        return Task.CompletedTask;
    }

    private void HandleConfirmation(ulong deliveryTag, bool multiple, bool isSuccess)
    {
        if (multiple)
        {
            foreach (var key in _outstandingConfirms.Keys)
            {
                if (key <= deliveryTag)
                {
                    if (_outstandingConfirms.TryRemove(key, out var tcs))
                    {
                        tcs.TrySetResult(isSuccess);
                    }
                }
            }
        }
        else
        {
            if (_outstandingConfirms.TryRemove(deliveryTag, out var tcs))
            {
                tcs.TrySetResult(isSuccess);
            }
        }
    }

    private Task OnBasicReturnAsync(object sender, BasicReturnEventArgs e)
    {
        string body = Encoding.UTF8.GetString(e.Body.ToArray());
        _logger.LogCritical("[Unroutable Message] Message arrived at exchange but could not be routed to any queue! ReplyCode: {Code}, Reason: {Text}, Exchange: {Exchange}, RoutingKey: {Key}, Payload: {Payload}",
            e.ReplyCode, e.ReplyText, e.Exchange, e.RoutingKey, body);
        return Task.CompletedTask;
    }

    #endregion

    public async Task DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error thrown while safely shutting down async channel execution contexts.");
        }
        finally
        {
            _channelLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        Task.Run(async () => await DisposeAsync());
    }
}