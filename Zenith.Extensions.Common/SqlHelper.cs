using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Zenith.Extensions.Common
{
    public class SqlHelper
    {
        protected readonly string _connectionString;

        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServer");
        }

        public SqlHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public object ExecuteScalar(string sql, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (param != null) sql = sql + " OPTION(RECOMPILE)";
                return conn.ExecuteScalar(sql, param, commandTimeout: 300);
            }
        }

        public T ExecuteScalar<T>(string sql, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (param != null) sql = sql + " OPTION(RECOMPILE)";
                return conn.ExecuteScalar<T>(sql, param, commandTimeout: 300);
            }
        }

        public List<T> Query<T>(string sql, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (param != null) sql = sql + " OPTION(RECOMPILE)";
                return conn.Query<T>(sql, param, commandTimeout: 300).AsList();
            }
        }

        public (List<T> list, int totalCount) QueryByPage<T>(string sql, PageInfo pageInfo, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sqlTotal = $"SELECT COUNT(*) FROM ({sql}) tmp";
                if (param != null) sqlTotal = sqlTotal + " OPTION(RECOMPILE)";
                var totalCount = conn.ExecuteScalar<int>(sqlTotal, param, commandTimeout: 300);
                if (totalCount == 0) return (new List<T>(), totalCount);

                int startIndex = pageInfo.PageSize * pageInfo.PageIndex - pageInfo.PageSize;
                sql = $"{sql} ORDER BY {pageInfo.OrderBy} OFFSET {startIndex} ROW FETCH NEXT {pageInfo.PageSize} ROWS ONLY";
                if (param != null) sql = sql + " OPTION(RECOMPILE)";
                var list = conn.Query<T>(sql, param, commandTimeout: 300);
                return (list.AsList(), totalCount);
            }
        }
    }

    public class PageInfo
    {
        public int PageSize { get; set; } = 10;

        public int PageIndex { get; set; } = 1;

        public string OrderBy { get; set; }
    }
}
