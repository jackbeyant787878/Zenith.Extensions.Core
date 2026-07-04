using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Zenith.Extensions.Utils
{
    public class ConfigUtil
    {
        public static string EnvironmentName { get; private set; }

        public static IConfiguration Configuration { get; private set; }

        static ConfigUtil()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(EnvironmentName)) EnvironmentName = "Production";
            Configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = $"appsettings.json", ReloadOnChange = true })
                .Add(new JsonConfigurationSource { Path = $"appsettings.{EnvironmentName}.json", ReloadOnChange = true })
                .Build();
        }

        public static string GetDbConnStr(string database)
        {
            if (!string.IsNullOrEmpty(Configuration.GetConnectionString(database)))
            {
                return Configuration.GetConnectionString(database);
            }

            return string.Empty;
        }

        public static string GetRedisConnStr(string name)
        {
            if (!string.IsNullOrEmpty(Configuration.GetConnectionString(name)))
            {
                return Configuration.GetConnectionString(name);
            }
            return string.Empty;
        }

        public static string GetValue(string key)
        {
            if (!string.IsNullOrEmpty(Configuration.GetSection(key).Value))
            {
                return Configuration.GetSection(key).Value;
            }
            return string.Empty;
        }
    }
}
