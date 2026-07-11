namespace Zenith.Extensions.Elasticsearch
{
    public class LoggerFactory<T> where T : Log
    {
        public static ILogger<AccessLog> GetAccessLogger(string domain)
        {
            return new Logger<AccessLog>(domain, "access");
        }

        public static ILogger<ErrorLog> GetErrorLogger(string domain)
        {
            return new ErrorLogger(domain);
        }

        public static ILogger<T> GetCustomerLogger(string domain, string logType)
        {
            return new Logger<T>(domain, logType);
        }
    }
}
