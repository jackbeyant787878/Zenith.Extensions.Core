using Newtonsoft.Json;
namespace Zenith.Extensions.Common

{
    public class ConfigCenterHelper
    {
        private static readonly Dictionary<DbClient, string> _connStrings = new Dictionary<DbClient, string>();
        private static readonly object _locker = new object();

        public static string GetDbConnStr(DbClient client)
        {
            lock (_locker)
            {
                if (_connStrings.TryGetValue(client, out string connStr) && !string.IsNullOrEmpty(connStr))
                {
                    return connStr;
                }
                else
                {
                    string product = ConfigurationHelper.GetValue($"ConfigCenter:{client}:Product");
                    string env = ConfigurationHelper.GetValue($"ConfigCenter:{client}:Env");
                    string uri = $"http://configcenter.pacvue.cn:9110//api/dbconfig?product={product}&env={env}";
                    var httpClient = new HttpClient();
                    var response = httpClient.GetStringAsync(uri).Result;
                    var result = JsonConvert.DeserializeObject<List<SqlConnResponse>>(response);
                    connStr = result.FirstOrDefault(x => x.ConnectionName == "sqlserver").ConnectionString;
                    _connStrings[client] = connStr;
                    return connStr;
                }
            }
        }
    }

    public enum DbClient
    {
        Amazon,
        AmazonCN,
        Walmart,
        Instacart,
        Ebay,
        Criteo
    }

    public class SqlConnResponse
    {
        public string ConnectionName { get; set; }

        public string ConnectionString { get; set; }
    }
}
