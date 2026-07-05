
namespace Zenith.Extensions.RabbitMQ;

/// <summary>
/// Standard interface for the high-availability RabbitMQ Producer.
/// </summary>
public interface IRabbitMQProducer : IDisposable
{
    /// <summary>
    /// Publishes a message using Direct Exchange (Classic Work Queue pattern).
    /// </summary>
    Task<bool> PublishAsync<T>(string exchange, string routingKey, T message, PublishOptions? options = null);

    /// <summary>
    /// Publishes a message using Fanout Exchange (Broadcast pattern).
    /// </summary>
    Task<bool> PublishFanoutAsync<T>(string exchange, T message, PublishOptions? options = null);

    /// <summary>
    /// Publishes a message using Topic Exchange (Wildcard matching pattern).
    /// </summary>
    Task<bool> PublishTopicAsync<T>(string exchange, string routingKey, T message, PublishOptions? options = null);

    /// <summary>
    /// Publishes a message using Headers Exchange (Attributes matching pattern).
    /// </summary>
    Task<bool> PublishHeadersAsync<T>(string exchange, Dictionary<string, object> headers, T message, PublishOptions? options = null);
}