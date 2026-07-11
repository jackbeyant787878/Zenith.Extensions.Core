# Zenith\.Extensions\.RabbitMQ

# Zenith\.Extensions\.RabbitMQ

A **high\-performance, fully asynchronous, thread\-safe RabbitMQ SDK** for \.NET, built on top of the official`RabbitMQ.Client v7.x` async API\. This library encapsulates connection persistence, reliable message publishing, multi\-exchange routing, delay queue \& dead\-letter infrastructure, and full async consumer bootstrap\. It eliminates repetitive RabbitMQ topology code and provides production\-ready MQ capabilities for modern \.NET applications\.

## ✨ Key Features

- **Fully Async \& Modern**: 100% async implementation compatible with RabbitMQ\.Client v7\+ new async channel/connection API

- **Persistent Connection Management**: Thread\-safe singleton physical connection, automatic reconnection \& connection state monitoring

- **Full Exchange Type Support**: Built\-in support for **Direct / Fanout / Topic / Headers** routing patterns

- **Reliable Message Publishing**: Publisher confirms, mandatory routing check, unroutable message capture, 10s publish timeout protection

- **Delay \& Dead\-Letter Queue**: One\-click DLX infrastructure declaration, support for queue\-level TTL and message\-level TTL delay messages

- **Generic Auto Serialization**: Automatic JSON serialize/deserialize for custom entity messages

- **Dynamic Consumer Bootstrap**: Quick work queue consumer creation, runtime adjustable QoS prefetch count

- **Null Safety \& Error Logging**: Built\-in exception handling, structured logging, and unroutable message critical logs

- **DI Friendly \& Disposable**: Implements `IDisposable/IAsyncDisposable`, perfectly fits ASP\.NET Core DI lifecycle

## 📦 Installation

### \.NET CLI

```Plain Text
dotnet add package Zenith.Extensions.RabbitMQ
```

### NuGet Package Manager

```Plain Text
Install-Package Zenith.Extensions.RabbitMQ
```

## 📚 Full Feature API List

### 1\. Multi\-Type Exchange Publisher

- `PublishAsync<T>` : Direct Exchange / Work Queue pattern

- `PublishFanoutAsync<T>` : Fanout Exchange / Broadcast pattern

- `PublishTopicAsync<T>` : Topic Exchange / Wildcard routing pattern

- `PublishHeadersAsync<T>` : Headers Exchange / Attribute matching pattern

### 2\. Advanced Publish Options

- Custom message TTL \(per\-message delay\)

- Alternate Exchange for unroutable messages

- Custom header dictionary for headers routing

- Persistent message delivery mode

### 3\. Topology Management

- Unified topology setup: declare exchange \+ durable queue \+ queue binding

- Built\-in DLX \(Dead Letter Exchange\) infrastructure declaration

- Delay queue creation support \(queue\-level \& message\-level TTL\)

### 4\. Persistent Connection \& Channel

- Thread\-safe connection recovery \& health check

- Async channel creation with publisher confirmation enabled

- Auto clean\-up connection/channel on shutdown

### 5\. Async Consumer Framework

- Quick bootstrap work queue consumer

- Runtime update QoS prefetch count

- Manual ACK mode to ensure message reliability

- Complete consumer context lifecycle management

## 🚀 Quick Integration \(ASP\.NET Core\)

### Step 1: Register Services in Program\.cs

```Plain Text
// RabbitMQ Connection Factory
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = "127.0.0.1",
        UserName = "guest",
        Password = "guest",
        Port = AmqpTcpEndpoint.UseDefaultPort,
        VirtualHost = "/",
        AutomaticRecoveryEnabled = true
    };
});

// Persistent Connection Manager (required by RabbitMQProducer)
builder.Services.AddSingleton<RabbitMQPersistentConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
    return new RabbitMQPersistentConnection(factory, logger);
});

// Fixed: Complete dependency injection for RabbitMQProducer
builder.Services.AddScoped<IRabbitMQProducer, RabbitMQProducer>(sp =>
{
    var persistentConn = sp.GetRequiredService<RabbitMQPersistentConnection>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQProducer>>();
    return new RabbitMQProducer(persistentConn, logger);
});
```

### Step 2: Publish Message Example

```Plain Text
public class MessageService
{
    private readonly IRabbitMQProducer _rabbitProducer;

    public MessageService(IRabbitMQProducer rabbitProducer)
    {
        _rabbitProducer = rabbitProducer;
    }

    public async Task SendDirectMessage()
    {
        var message = new { Title = "Hello Rabbit", Time = DateTime.Now };
        
        // Publish Direct Message
        bool result = await _rabbitProducer.PublishAsync(
            exchange: "direct.exchange",
            routingKey: "work.key",
            message: message);
    }

    public async Task SendBroadcastMessage()
    {
        var msg = new { Content = "Broadcast Notification" };
        await _rabbitProducer.PublishFanoutAsync("fanout.exchange", msg);
    }
}
```

### Step 3: Delay \& Dead Letter Queue Usage

```Plain Text
var publisher = new RabbitMQAsyncPublisher("127.0.0.1", "guest", "guest");
await publisher.InitializeAsync();

// Declare DLX infrastructure
await publisher.DeclareDeadLetterInfrastructureAsync("dlx.exchange", "dlx.queue", "dlx.key");

// Declare delay queue (5000ms global TTL)
await publisher.DeclareDelayQueueAsync(
    bizExchange: "biz.exchange",
    bizQueue: "biz.queue",
    bizRoutingKey: "biz.key",
    dlxExchange: "dlx.exchange",
    dlxRoutingKey: "dlx.key",
    ttlMs: 5000);

// Publish delay message
await publisher.PublishAsync("biz.exchange", "biz.key", "Delay Message Content", messageTtlMs: 3000);
```

### Step 4: Consumer Bootstrap

```Plain Text
var bootstrap = new RabbitMQConsumerBootstrap("127.0.0.1", "guest", "guest");
var consumerContext = await bootstrap.CreateWorkQueueConsumerAsync(
    queueName: "biz.queue",
    prefetchCount: 10,
    exchangeName: "biz.exchange",
    routingKey: "biz.key");

// Subscribe message
consumerContext.Consumer.ReceivedAsync += async (sender, args) =>
{
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    // Handle your business logic here

    // Manual ACK
    await args.BasicAckAsync(multiple: false);
};
```

## 🔧 Supported Runtime \& Dependencies

- **Target Frameworks**: \.NET 6 / \.NET 7 / \.NET 8 / \.NET 9\+

- **Core Dependency**: RabbitMQ\.Client \(v7\.x latest\)

- **Built\-in Serializer**: System\.Text\.Json

- **DI Support**: Microsoft\.Extensions\.DependencyInjection / Logging

## 💡 Production Best Practices

- Register `RabbitMQPersistentConnection` as **Singleton** to avoid repeated TCP connection creation

- Always set message/queue TTL to prevent message accumulation

- Use manual ACK mode for consumers to ensure message consistency

- Enable Publisher Confirmation to guarantee message delivery reliability

- Catch unroutable messages via built\-in return callback for abnormal monitoring

## 📄 License

Open\-source, free for commercial and non\-commercial usage, modification and distribution\.

