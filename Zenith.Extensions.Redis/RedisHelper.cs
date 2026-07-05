using System.Text.Json;
using StackExchange.Redis;
namespace Zenith.Extensions.Redis;
public class RedisHelper : IRedisHelper
{
    private readonly IDatabase _db;

    /// <summary>
    /// Initializes the helper using StackExchange.Redis.
    /// The ConnectionMultiplexer instance should ideally be managed as a Singleton via DI.
    /// </summary>
    public RedisHelper(IConnectionMultiplexer multiplexer, int dbIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        _db = multiplexer.GetDatabase(dbIndex);
    }

    private string Serialize<T>(T value) =>
        value is string str ? str : JsonSerializer.Serialize(value);

    private T? Deserialize<T>(string? value)
    {
        if (string.IsNullOrEmpty(value)) return default;
        if (typeof(T) == typeof(string)) return (T)(object)value;
        return JsonSerializer.Deserialize<T>(value);
    }

    // === String Operations ===

    public bool Set(string key, string value, TimeSpan expiry) =>
        _db.StringSet(key, value, expiry);

    public Task<bool> SetAsync(string key, string value, TimeSpan expiry) =>
        _db.StringSetAsync(key, value, expiry);

    public bool Set<T>(string key, T value, TimeSpan expiry) =>
        _db.StringSet(key, Serialize(value), expiry);

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan expiry) =>
        _db.StringSetAsync(key, Serialize(value), expiry);

    public string? Get(string key) => _db.StringGet(key);

    public async Task<string?> GetAsync(string key) => await _db.StringGetAsync(key);

    public T? Get<T>(string key) => Deserialize<T>(_db.StringGet(key));

    public async Task<T?> GetAsync<T>(string key) => Deserialize<T>(await _db.StringGetAsync(key));

    public bool Delete(string key) => _db.KeyDelete(key);

    public Task<bool> DeleteAsync(string key) => _db.KeyDeleteAsync(key);

    // === Set Operations ===

    public T? SPop<T>(string key) => Deserialize<T>(_db.SetPop(key));

    public async Task<T?> SPopAsync<T>(string key) => Deserialize<T>(await _db.SetPopAsync(key));

    public long SRem<T>(string key, T[] members) =>
        _db.SetRemove(key, members.Select(m => (RedisValue)Serialize(m)).ToArray());

    public Task<long> SRemAsync<T>(string key, T[] members) =>
        _db.SetRemoveAsync(key, members.Select(m => (RedisValue)Serialize(m)).ToArray());

    public long SAdd<T>(string key, T[] members) =>
        _db.SetAdd(key, members.Select(m => (RedisValue)Serialize(m)).ToArray());

    public Task<long> SAddAsync<T>(string key, T[] members) =>
        _db.SetAddAsync(key, members.Select(m => (RedisValue)Serialize(m)).ToArray());

    public bool SIsMember(string key, object member) => _db.SetContains(key, Serialize(member));

    public Task<bool> SIsMemberAsync(string key, object member) => _db.SetContainsAsync(key, Serialize(member));

    public T[] SMembers<T>(string key) =>
        _db.SetMembers(key).Select(m => Deserialize<T>(m)!).ToArray();

    public async Task<T[]> SMembersAsync<T>(string key) =>
        (await _db.SetMembersAsync(key)).Select(m => Deserialize<T>(m)!).ToArray();

    // === Hash Operations ===

    public T? HGet<T>(string key, string field) => Deserialize<T>(_db.HashGet(key, field));

    public async Task<T?> HGetAsync<T>(string key, string field) => Deserialize<T>(await _db.HashGetAsync(key, field));

    public bool HSet(string key, string field, object value) => _db.HashSet(key, field, Serialize(value));

    public Task<bool> HSetAsync(string key, string field, object value) => _db.HashSetAsync(key, field, Serialize(value));

    public long HDel(string key, string[] fields) => _db.HashDelete(key, fields.Select(f => (RedisValue)f).ToArray());

    public Task<long> HDelAsync(string key, string[] fields) => _db.HashDeleteAsync(key, fields.Select(f => (RedisValue)f).ToArray());
}