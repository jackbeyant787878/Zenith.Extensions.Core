using log4net;
using Newtonsoft.Json;

namespace Zenith.Extensions.Elasticsearch
{
    public class Log4NetLogger<T> : ILogger<T> where T : Log
    {
        private readonly string _domain;
        private readonly string _loggerName;

        public Log4NetLogger(string domain = "zenith", string loggerName = "default")
        {
            _domain = domain;
            _loggerName = loggerName;
        }

        void ILogger<T>.Log(T data)
        {
            var logger = LogManager.GetLogger(_domain, _loggerName);
            logger.Info(JsonConvert.SerializeObject(data));
        }

        Task ILogger<T>.LogAsync(T data)
        {
            throw new NotImplementedException();
        }
    }
}
