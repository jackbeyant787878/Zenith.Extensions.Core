using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Zenith.Extensions.RabbitMQ
{
    /// <summary>
    /// Acts as a lifecycle container for the active consumer, maintaining the underlying channel and connection.
    /// </summary>
    public class RabbitMQConsumerContext : IAsyncDisposable
    {
        public IConnection Connection { get; }
        public IChannel Channel { get; }
        public AsyncEventingBasicConsumer Consumer { get; }

        public RabbitMQConsumerContext(IConnection connection, IChannel channel, AsyncEventingBasicConsumer consumer)
        {
            Connection = connection;
            Channel = channel;
            Consumer = consumer;
        }

        /// <summary>
        /// Dynamically alters the Prefetch Count (QoS) at runtime if needed.
        /// </summary>
        public async Task UpdatePrefetchCountAsync(ushort newPrefetchCount)
        {
            // RabbitMQ allows changing QOS on the fly for active channels
            await Channel.BasicQosAsync(prefetchSize: 0, prefetchCount: newPrefetchCount, global: false);
        }

        public async ValueTask DisposeAsync()
        {
            if (Channel != null) await Channel.CloseAsync();
            if (Connection != null) await Connection.CloseAsync();
        }
    }

    public class RabbitMQConsumerBootstrap
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQConsumerBootstrap(string host, string user, string pass, int port = 5672, string virtualHost = "/")
        {
            _factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass,
                AutomaticRecoveryEnabled = true,
                Port = port,
                VirtualHost = virtualHost
            };
        }

        /// <summary>
        /// Bootstraps the entire queue infrastructure and returns an actionable Consumer Context.
        /// </summary>
        /// <param name="queueName">The target queue to listen to.</param>
        /// <param name="prefetchCount">Dynamic capacity controller (QoS).</param>
        /// <param name="exchangeName">Optional exchange to declare and bind.</param>
        /// <param name="routingKey">Optional routing key for the exchange binding.</param>
        /// <param name="exchangeType">Exchange type defaults to Fanout if exchangeName is supplied.</param>
        public async Task<RabbitMQConsumerContext> CreateWorkQueueConsumerAsync( string queueName,ushort prefetchCount = 1,string? exchangeName = null,string? routingKey = null,string exchangeType = ExchangeType.Fanout)
        {
            var connection = await _factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            // 1. Handle optional exchange declarations and bindings
            if (!string.IsNullOrEmpty(exchangeName))
            {
                await channel.ExchangeDeclareAsync(exchange: exchangeName, type: exchangeType, durable: true, autoDelete: false);
                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
                await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey ?? string.Empty);
            }
            else
            {
                // Default simple Work Queue declaration
                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            }


            // 2. Set the initial configurable BasicQos
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

            // 3. Initialize the asynchronous consumer
            var consumer = new AsyncEventingBasicConsumer(channel);

            // 4. Start consuming (autoAck is explicitly set to false to respect QoS)
            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            return new RabbitMQConsumerContext(connection, channel, consumer);
        }
    }
}