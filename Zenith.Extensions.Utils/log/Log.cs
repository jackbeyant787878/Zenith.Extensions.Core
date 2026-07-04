using Newtonsoft.Json;
namespace Zenith.Extensions.Utils
{
    public class Log
    {
        [JsonProperty("@timestamp")]
        public DateTime TimeStamp { get; } = DateTime.UtcNow;

        /// <summary>
        /// this field will be your index in elasticsearch
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// indicate where your logs store
        /// </summary>
        public LogType LogType { get; set; }
    }

    public enum LogType
    {
        esOnly,
        localOnly,
        both
    }
}
