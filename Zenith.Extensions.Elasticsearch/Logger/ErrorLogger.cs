using System;
using System.Threading.Tasks;

namespace Zenith.Extensions.Elasticsearch
{
    public class ErrorLogger : ILogger<ErrorLog>
    {
        private ILogger<SimpleErrorLog> _logger;

        public ErrorLogger(string domain, string loggerName = "ErrorLogger")
        {
            _logger = new Logger<SimpleErrorLog>(domain, loggerName);
        }

        public void Log(ErrorLog data)
        {
            var log = Adapter(data.Ex);
            _logger.Log(log);
        }

        public async Task LogAsync(ErrorLog data)
        {
            var log = Adapter(data.Ex);
            await _logger.LogAsync(log);
        }

        private SimpleErrorLog Adapter(Exception ex)
        {
            var target = new SimpleException
            {
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Message = ex.Message,
            };
            var tmp = target;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                tmp.InnerException = new SimpleException
                {
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Message = ex.Message,
                };
                tmp = tmp.InnerException;
            }
            return new SimpleErrorLog
            {
                Ex = target
            };
        }
    }
}
