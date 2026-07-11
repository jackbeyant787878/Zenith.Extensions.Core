# Zenith\.Extensions\.Elasticsearch

A **production\-grade, high\-availability Elasticsearch helper library** for \.NET, built on the official modern `Elastic.Clients.Elasticsearch 9.0+` SDK\. This package provides encapsulated synchronous/asynchronous indexing, built\-in **circuit breaker failure protection**, environment\-based configuration, socket exhaustion prevention, and automatic service recovery — ready for high\-concurrency production logging and document indexing scenarios\.

<img width="1463" height="538" alt="image" src="https://github.com/user-attachments/assets/b9481198-24af-48cb-a1a8-ecf3a97bbd8d" />

## ✨ Core Features

- **Modern Elasticsearch 9\.x Support**: Fully adapted to the latest official client API and response validation rules

- **Built\-in Circuit Breaker**: Rolling 10s failure statistics, automatic trip \& 90s cooldown recovery to avoid avalanche failure

- **Global Singleton Client**: Single reusable Elasticsearch client to completely solve socket exhaustion under high QPS

- **Environment Variable Configuration**: Dynamic endpoint loading via `ELASTICSEARCH_URL`, with intelligent fallback default value

- **Dual Sync / Async API**: Provides both `LogAsync` and `Log` for flexible business usage

- **Automatic Index Normalization**: Auto convert index name to lowercase to comply with Elasticsearch 9\.x strict rules

- **Structured Logging Integration**: Built\-in ILogger error/warning/critical logs for troubleshooting

- **High Concurrency Safe**: Thread\-safe failure queue \& lock\-free state judgment, optimized for heavy traffic

## 📦 Installation

### \.NET CLI

```Plain Text
dotnet add package Zenith.Extensions.Elasticsearch
```

### NuGet Package Manager

```Plain Text
Install-Package Zenith.Extensions.Elasticsearch
```

## ⚙️ Environment Configuration

The library automatically reads the Elasticsearch connection address from environment variables:

- **Key**: `ELASTICSEARCH_URL`

- **Default fallback**: `http://localhost:9200`

- **Request Timeout**: Fixed 2 seconds for fast failure response

## 🚀 DI Integration \(ASP\.NET Core\)

Standard dependency injection registration with logger support:

```Plain Text
// Program.cs
builder.Services.AddScoped<ElasticSearchHelper>(sp =>
{
    var logger = sp.GetService<ILogger<ElasticSearchHelper>>();
    return new ElasticSearchHelper(logger);
});
```

## 📝 Usage Examples

### Asynchronous Indexing \(Recommended\)

```Plain Text
public class LogService
{
    private readonly ElasticSearchHelper _esHelper;

    public LogService(ElasticSearchHelper esHelper)
    {
        _esHelper = esHelper;
    }

    public async Task WriteLogAsync()
    {
        var logModel = new
        {
            Message = "System running log",
            CreateTime = DateTime.UtcNow,
            Level = "Info"
        };

        // Index name will auto convert to lowercase
        await _esHelper.LogAsync("system-log", logModel);
    }
}
```

### Synchronous Indexing

```Plain Text
_esHelper.Log("business-log", new { OrderId = 10001, Status = "Success" });
```

## 🛡️ Built\-in Circuit Breaker Mechanism

This library implements a **rolling window circuit breaker** to protect downstream Elasticsearch clusters in high\-concurrency scenarios:

- **Trigger Rule**: Trip circuit if **3 failures occur within 10 seconds**

- **Protection Duration**: 90 seconds cooling window

- **Auto Recovery**: Automatically close circuit and resume traffic after cooldown

- **Failure Isolation**: Reject new requests actively to prevent service avalanche

## 📚 API Reference

- `LogAsync<T>(string index, T data)`
Asynchronously write generic document data to specified index

- `Log<T>(string index, T data)`
Synchronously write generic document data to specified index

## 🔧 Supported Frameworks \& Dependencies

- **Target Frameworks**: \.NET 6 / \.NET 7 / \.NET 8 / \.NET 9\+

- **Core Dependency**: Elastic\.Clients\.Elasticsearch 9\.0\+

- **Logging**: Microsoft\.Extensions\.Logging

## 💡 Production Best Practices

- Set `ELASTICSEARCH_URL` environment variable in staging/production environments

- Prefer async `LogAsync` in web applications to improve throughput

- Leverage built\-in circuit breaker to avoid massive error cascades during ES cluster downtime

- Reuse helper instance via DI to maintain singleton ES client connection

## 📄 License

Open\-source library for commercial and non\-commercial use\. Free to reference, extend and optimize\.

> （注：部分内容可能由 AI 生成）
