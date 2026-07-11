using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Zenith.Extensions.RabbitMQ
{
    /// <summary>
    /// Advanced fully asynchronous publisher wrapper based on RabbitMQ.Client v7.x 
    /// (Supports Dead-Letter Exchange, Delay Queues, and Message Return Mechanisms).
    /// </summary>
    public class RabbitMQAsyncPublisher : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _isInitialized = false;

        public RabbitMQAsyncPublisher(string host, string user, string pass, int port = 5672, string virtualHost = "/")
        {
            _factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass,
                Port = port,
                VirtualHost = virtualHost,
                AutomaticRecoveryEnabled = true
            };
        }

        /// <summary>
        /// Initializes the connection and channel, enabling v7 fully asynchronous publisher tracking.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            // 1. Create the connection asynchronously
            _connection = await _factory.CreateConnectionAsync();

            // 2. Pass read-only options into the constructor to enable publisher confirmations and built-in tracking (replacing the legacy ConfirmSelect)
            var channelOptions = new CreateChannelOptions(publisherConfirmationsEnabled: true,publisherConfirmationTrackingEnabled: true);

            // 3. Create the channel asynchronously
            _channel = await _connection.CreateChannelAsync(channelOptions);

            // 4. Attach the v7 asynchronous return event listener (replacing the legacy synchronous BasicReturn event)
            _channel.BasicReturnAsync += OnMessageReturnedAsync;

            _isInitialized = true;
        }

        /// <summary>
        /// Core: Declares the dead-letter infrastructure (Dead-Letter Exchange + Dead-Letter Queue + Binding) in one go.
        /// </summary>
        public async Task DeclareDeadLetterInfrastructureAsync(string dlxExchange, string dlxQueue, string dlxRoutingKey)
        {
            EnsureInitialized();

            // Declare the direct dead-letter exchange
            await _channel!.ExchangeDeclareAsync(exchange: dlxExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
            // Declare the dead-letter queue
            await _channel.QueueDeclareAsync(queue: dlxQueue, durable: true, exclusive: false, autoDelete: false);
            // Bind the dead-letter queue to the dead-letter exchange
            await _channel.QueueBindAsync(queue: dlxQueue, exchange: dlxExchange, routingKey: dlxRoutingKey);
        }

        /// <summary>
        /// Core: Declares a business delay queue (achieving message delay by appending dead-letter arguments).
        /// </summary>
        /// <param name="ttlMs">Optional. If a value is provided, it configures [Queue-Level (Global) Delay]; if null, it relies on [Message-Level (Per-Message) Delay].</param>
        public async Task DeclareDelayQueueAsync(string bizExchange,string bizQueue,string bizRoutingKey,string dlxExchange,
            string dlxRoutingKey,int? ttlMs = null)
        {
            EnsureInitialized();

            // 1. Declare the business exchange
            await _channel!.ExchangeDeclareAsync(exchange: bizExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);

            // 2. Build core dead-letter arguments (v7 dictionary supports object? types)
            var arguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", dlxExchange },
                { "x-dead-letter-routing-key", dlxRoutingKey }
            };

            // 3. Corresponding to legacy DelayQueuePublisher: add global TTL to arguments if provided
            if (ttlMs.HasValue)
            {
                arguments.Add("x-message-ttl", ttlMs.Value);
            }

            // 4. Declare the business queue attached with dead-letter attributes
            await _channel.QueueDeclareAsync(queue: bizQueue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            await _channel.QueueBindAsync(queue: bizQueue, exchange: bizExchange, routingKey: bizRoutingKey);
        }

        /// <summary>
        /// Asynchronously publishes a message (fully integrated with mandatory routing check, per-message TTL, and confirmation tracking).
        /// </summary>
        /// <param name="messageTtlMs">Optional. Corresponds to the per-message expiration setting (unit: milliseconds).</param>
        /// <param name="mandatory">Defaults to true. If routing fails, it will trigger the BasicReturnAsync asynchronous callback.</param>
        public async Task PublishAsync(
            string exchange,
            string routingKey,
            string message,
            int? messageTtlMs = null,
            bool mandatory = true)
        {
            EnsureInitialized();

            var body = Encoding.UTF8.GetBytes(message);

            // v7 specification: Instantiate properties directly without channel.CreateBasicProperties
            var properties = new BasicProperties
            {
                Persistent = true // Mark message as persistent
            };

            // If it belongs to "Message-Level Delay", dynamically assign the Expiration property
            if (messageTtlMs.HasValue)
            {
                properties.Expiration = messageTtlMs.Value.ToString();
            }

            try
            {
                // Since Tracking is enabled, the await here will only return successfully after receiving an ACK from the broker.
                // If a NACK is encountered, or if the message cannot be routed with 'mandatory' set to true, it will throw a PublishException right here.
                await _channel!.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: mandatory,
                    basicProperties: properties,
                    body: body
                );
            }
            catch (PublishException ex)
            {
                // In production, handle logging, moving to a retry queue, or triggering alerts here
                Console.WriteLine($"[Publish Exception] Message delivery failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronous message return event callback for v7.
        /// </summary>
        private Task OnMessageReturnedAsync(object sender, BasicReturnEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"⚠️ [Returned Message] Exchange: {e.Exchange}, RoutingKey: {e.RoutingKey}, ReplyCode: {e.ReplyCode}, Reason: {e.ReplyText}");
            Console.WriteLine($"Returned Content: {msg}");

            return Task.CompletedTask;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Please call InitializeAsync() first to establish the network stream!");
        }

        /// <summary>
        /// Gracefully releases all connection and channel resources fully asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                _channel.BasicReturnAsync -= OnMessageReturnedAsync;
                await _channel.CloseAsync();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}