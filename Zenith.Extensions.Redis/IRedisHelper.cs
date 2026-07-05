
namespace Zenith.Extensions.Redis;
public interface IRedisHelper
{
    // === String Operations ===
    bool Set(string key, string value, TimeSpan expiry);
    Task<bool> SetAsync(string key, string value, TimeSpan expiry);
    bool Set<T>(string key, T value, TimeSpan expiry);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan expiry);
    string? Get(string key);
    Task<string?> GetAsync(string key);
    T? Get<T>(string key);
    Task<T?> GetAsync<T>(string key);
    bool Delete(string key);
    Task<bool> DeleteAsync(string key);

    // === Set Operations ===
    T? SPop<T>(string key);
    Task<T?> SPopAsync<T>(string key);
    long SRem<T>(string key, T[] members);
    Task<long> SRemAsync<T>(string key, T[] members);
    long SAdd<T>(string key, T[] members);
    Task<long> SAddAsync<T>(string key, T[] members);
    bool SIsMember(string key, object member);
    Task<bool> SIsMemberAsync(string key, object member);
    T[] SMembers<T>(string key);
    Task<T[]> SMembersAsync<T>(string key);

    // === Hash Operations ===
    T? HGet<T>(string key, string field);
    Task<T?> HGetAsync<T>(string key, string field);
    bool HSet(string key, string field, object value);
    Task<bool> HSetAsync(string key, string field, object value);
    long HDel(string key, string[] fields);
    Task<long> HDelAsync(string key, string[] fields);
}