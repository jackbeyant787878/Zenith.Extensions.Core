using System;
using System.Threading.Tasks;

namespace Zenith.Extensions.Elasticsearch
{
    public class Logger<T> : ILogger<T> where T : Log
    {
        private readonly string _domain;
        private readonly string _logType;

        public Logger(string domain = "pacvue", string logType = "default")
        {
            _domain = domain;
            _logType = logType;
        }

        public virtual void Log(T data)
        {
            var helper = new ElasticSearchHelper();
            helper.Log(INDEX, data);
        }

        public virtual async Task LogAsync(T data)
        {
            var helper = new ElasticSearchHelper();
            await helper.LogAsync(INDEX, data);
        }

        private string INDEX
        {
            get
            {
                return $"{_domain.ToLower()}-{_logType.ToLower()}";
            }
        }
    }
}
