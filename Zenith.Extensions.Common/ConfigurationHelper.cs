using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace Zenith.Extensions.Common
{
    public class ConfigurationHelper
    {
        public static string EnvironmentName { get; set; }

        public static IConfiguration Configuration { get; private set; }

        static ConfigurationHelper()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(EnvironmentName)) EnvironmentName = "Production";
            Configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = $"appsettings.json", ReloadOnChange = true })
                .Add(new JsonConfigurationSource { Path = $"appsettings.{EnvironmentName}.json", ReloadOnChange = true })
                .Build();
        }

        public static string GetValue(string sectionName)
        {
            return Configuration.GetSection(sectionName).Value; ;
        }

        public static IConfiguration GetSection(string sectionName)
        {
            var section = Configuration.GetSection(sectionName);
            return section;
        }
    }
}
