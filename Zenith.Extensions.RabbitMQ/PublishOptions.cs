
namespace Zenith.Extensions.RabbitMQ;
/// <summary>
/// Advanced configurations for message publishing.
/// </summary>
public class PublishOptions
{
    /// <summary>
    /// Alternate exchange for unroutable messages.
    /// </summary>
    public string? AlternateExchange { get; set; }

    /// <summary>
    /// Message Time-To-Live (TTL) in milliseconds.
    /// </summary>
    public int? MessageTtl { get; set; }

    /// <summary>
    /// Key-value pairs for Headers Exchange matching routing.
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }
}