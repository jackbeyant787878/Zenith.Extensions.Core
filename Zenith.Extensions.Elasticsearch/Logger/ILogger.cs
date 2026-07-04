using System.Threading.Tasks;

namespace Zenith.Extensions.Elasticsearch
{
    public interface ILogger<T> where T : Log
    {
        void Log(T data);
        Task LogAsync(T data);
    }
}
