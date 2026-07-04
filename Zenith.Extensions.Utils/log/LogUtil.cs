using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Zenith.Extensions.Utils
{
    public class LogUtil
    {
        static readonly IProducer<Null, string> _producer;
        static readonly string _topic;
        static readonly JsonSerializerSettings _jsonSettings;

        static LogUtil()
        {
            string servers = ConfigUtil.GetValue("logUtil:kafka:servers");
            _topic = ConfigUtil.GetValue("logUtil:topic");
            var config = new ProducerConfig { BootstrapServers = servers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _jsonSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _jsonSettings.Converters.Add(new LongToStringConverter());
        }

        public static void Log<T>(T log) where T : Log
        {
            Log(JsonConvert.SerializeObject(log, _jsonSettings));
        }

        public static void LogScheduledJob(ScheduledJobLog logItem)
        {
            Log(logItem);
        }

        public static void LogApiCall(ApiCallLog logItem)
        {
            Log(logItem);
        }

        public static async Task LogAsync<T>(T log) where T : Log
        {
            await LogAsync(JsonConvert.SerializeObject(log, _jsonSettings));
        }

        private static void Log(string message)
        {
            _producer.Produce(_topic, new Message<Null, string> { Value = message });
        }

        private static async Task LogAsync(string message)
        {
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        }
    }
}
