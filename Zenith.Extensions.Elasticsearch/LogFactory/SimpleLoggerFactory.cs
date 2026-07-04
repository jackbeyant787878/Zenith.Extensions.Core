namespace Zenith.Extensions.Elasticsearch
{
    public class SimpleLoggerFactory
    {
        public static ILogger<AccessLog> GetAccessLogger(string domain, string loggerName = "AccessLogger")
        {
            return new Logger<AccessLog>(domain, loggerName);
        }

        public static ILogger<ErrorLog> GetErrorLogger(string domain, string loggerName = "ErrorLogger")
        {
            return new ErrorLogger(domain, loggerName);
        }

        public static ILogger<T> GetCustomerLogger<T>(string domain, string loggerName) where T : Log
        {
            return new Logger<T>(domain, loggerName);
        }

        public static ILogger<AccessLog> GetLog4netAccessLogger(string domain, string loggerName = "AccessLogger")
        {
            return new Log4NetLogger<AccessLog>(domain, loggerName);
        }

        public static ILogger<ErrorLog> GetLog4netErrorLogger(string domain, string loggerName = "ErrorLogger")
        {
            return new Log4NetLogger<ErrorLog>(domain, loggerName);
        }

        public static ILogger<T> GetLog4netCustomerLogger<T>(string domain, string loggerName) where T : Log
        {
            return new Log4NetLogger<T>(domain, loggerName);
        }
    }
}
