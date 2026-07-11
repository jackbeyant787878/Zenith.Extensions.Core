# Zenith\.Extensions\.Redis \- NuGet README

# Zenith\.Extensions\.Redis

A lightweight and high\-performance Redis extension library built on top of **StackExchange\.Redis** for \.NET projects\. It provides a clean, intuitive API with full sync/async method coverage, encapsulating the most commonly used Redis data structures including String, Set, and Hash\. Greatly simplifies Redis operation code for \.NET developers\.

## ✨ Key Features

- **Simplified Encapsulation**: Wraps native StackExchange\.Redis low\-level APIs with straightforward and human\-friendly method semantics

- **Generic Type Support**: Built\-in automatic JSON serialization/deserialization for custom objects, no manual conversion required

- **Dual Sync/Async APIs**: All operations provide both synchronous and asynchronous methods to fit diverse business scenarios

- **Core Data Structure Coverage**: Fully supports the most frequently used Redis structures: String, Set, Hash

- **Null Safety**: Built\-in null value handling, compatible with string and entity object storage, no redundant boxing/unboxing logic

- **DI Friendly**: Perfectly adapts to ASP\.NET Core dependency injection architecture, supports singleton Redis connection management

## 📦 Installation

### NuGet Package Manager

```Plain Text
Install-Package Zenith.Extensions.Redis
```

### \.NET CLI

```Plain Text
dotnet add package Zenith.Extensions.Redis
```

## 🚀 Quick Start

### 1\. Service Registration \(ASP\.NET Core\)

It is recommended to register `IConnectionMultiplexer` as a singleton to reuse Redis connections and avoid performance overhead:

```Plain Text
// Program.cs / Startup.cs
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect("127.0.0.1:6379,password=YourPassword,allowAdmin=true"));

// Register Redis Helper
builder.Services.AddScoped<IRedisHelper, RedisHelper>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    // Specify database index, default: 0
    return new RedisHelper(multiplexer, dbIndex: 0);
});
```

### 2\. Usage via Dependency Injection

```Plain Text
public class DemoService
{
    private readonly IRedisHelper _redisHelper;

    public DemoService(IRedisHelper redisHelper)
    {
        _redisHelper = redisHelper;
    }

    // Use Redis methods in your business logic
}
```

## 📋 Usage Examples

### 1\. String Operations

Supports string and custom object storage with automatic serialization and expiration configuration\.

```Plain Text
// Store string value (sync)
_redisHelper.Set("UserName", "Zenith", TimeSpan.FromMinutes(30));

// Store custom entity (auto serialization)
var user = new User { Id = 1, Name = "TestUser" };
_redisHelper.Set("User:1", user, TimeSpan.FromHours(1));

// Get values
var userName = _redisHelper.Get("UserName");
var userInfo = _redisHelper.Get<User>("User:1");

// Async operations
await _redisHelper.SetAsync("Async:Key", "AsyncData", TimeSpan.FromMinutes(10));
var asyncData = await _redisHelper.GetAsync<string>("Async:Key");

// Delete key
_redisHelper.Delete("UserName");
await _redisHelper.DeleteAsync("User:1");
```

### 2\. Set Operations

Redis unordered set operations, support batch add/remove, membership check, random pop and full member query\.

```Plain Text
// Batch add set members
_redisHelper.SAdd("Set:UserIds", new[] { 1, 2, 3, 4 });

// Check if member exists
var isExist = _redisHelper.SIsMember("Set:UserIds", 2);

// Get all set members
var userIds = _redisHelper.SMembers<int>("Set:UserIds");

// Random pop one member
var popId = _redisHelper.SPop<int>("Set:UserIds");

// Batch remove members
var removedCount = _redisHelper.SRem("Set:UserIds", new[] { 3, 4 });

// Async set operations
await _redisHelper.SAddAsync("Set:Async", new[] { "A", "B", "C" });
var asyncMembers = await _redisHelper.SMembersAsync<string>("Set:Async");
```

### 3\. Hash Operations

Suitable for object field storage, supports single field read/write and batch field deletion\.

```Plain Text
// Set hash field value
_redisHelper.HSet("Hash:User:1", "Name", "Tom");
_redisHelper.HSet("Hash:User:1", "Age", 25);

// Get hash field value
var userName = _redisHelper.HGet<string>("Hash:User:1", "Name");
var userAge = _redisHelper.HGet<int>("Hash:User:1", "Age");

// Batch delete hash fields
var deleteCount = _redisHelper.HDel("Hash:User:1", new[] { "Age" });

// Async hash operations
await _redisHelper.HSetAsync("Hash:Async:1", "Phone", "13800138000");
var phone = await _redisHelper.HGetAsync<string>("Hash:Async:1", "Phone");
```

## 📚 API Reference

### String Operations

- `Set/SetAsync`: Store string or generic object with expiration time

- `Get/GetAsync`: Get string value by key

- `Get<T>/GetAsync<T>`: Get and deserialize value to target generic type

- `Delete/DeleteAsync`: Remove specified key from Redis

### Set Operations

- `SAdd/SAddAsync`: Batch add members to set

- `SRem/SRemAsync`: Batch remove members from set

- `SIsMember/SIsMemberAsync`: Check if a member exists in set

- `SMembers/SMembersAsync`: Get all members of the set

- `SPop/SPopAsync`: Randomly pop one member from set

### Hash Operations

- `HSet/HSetAsync`: Set value for specified hash field

- `HGet/HGetAsync`: Get and deserialize value from specified hash field

- `HDel/HDelAsync`: Batch delete hash fields

## ⚙️ Dependencies \& Targets

- **Target Frameworks**: \.NET 6 / \.NET 7 / \.NET 8\+

- **Core Dependency**: StackExchange\.Redis \(latest stable version\)

- **Serializer**: Native System\.Text\.Json, no third\-party dependencies

## 💡 Best Practices

1. **Reuse Connection**: Register `IConnectionMultiplexer` as singleton to avoid excessive Redis connection creation

2. **Prefer Async**: Use async methods in web applications to improve thread throughput

3. **Set Expiration**: Always set a reasonable expiration time for cache keys to prevent Redis memory overflow

4. **Built\-in Null Safety**: No extra null judgment required in business code

## 📄 License

This project is open\-source\. Free for both commercial and non\-commercial use, modification and extension\.

