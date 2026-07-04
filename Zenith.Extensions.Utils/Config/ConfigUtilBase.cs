using Microsoft.Extensions.Configuration;
namespace Zenith.Extensions.Utils
{
    class ConfigUtilBase
    {
        private static List<DbConnection> GetConnections(IConfiguration configuration)
        {
            return configuration.GetSection("Connection").Get<List<DbConnection>>();
        }

        private static DbConnectionString GetDbConnection(IConfiguration configuration, string database)
        {
            var conns = configuration.GetSection("ConnectionString")
                .Get<List<DbConnectionString>>();
            var conn = conns?.FirstOrDefault(x => x.Database == database);
            if (conn == null) return null;
            conn.Connection = GetConnections(configuration)?
                .FirstOrDefault(x => x.Name == conn.ConnectionName);
            return conn;
        }

        public static string GetDbConnStr(IConfiguration configuration, string database)
        {
            var conn = GetDbConnection(configuration, database);
            if (conn == null) return null;
            return $"data source={conn.Connection.Host};initial catalog={conn.Database};persist security info=True;user id={conn.User};password={conn.Password};multipleactiveresultsets=True;";
        }

        public static string GetRedisConnStr(IConfiguration configuration, string name)
        {
            var conns = configuration.GetSection("Redis")
                .Get<List<DbConnection>>();
            var conn = conns?.FirstOrDefault(x => x.Name == name);
            if (conn == null) return null;
            return conn.Host;
        }


        public static string GetDbConnStrForEF(IConfiguration configuration, string database, string modelName)
        {
            var conn = GetDbConnection(configuration, database);
            return $@"metadata = res://*/{modelName}.csdl|res://*/{modelName}.ssdl|res://*/{modelName}.msl;provider=System.Data.SqlClient;provider connection string='data source={conn.Connection.Host};initial catalog={conn.Database};persist security info=True;user id={conn.User};password={conn.Password};MultipleActiveResultSets=True;App=EntityFramework'";
        }
    }
}
