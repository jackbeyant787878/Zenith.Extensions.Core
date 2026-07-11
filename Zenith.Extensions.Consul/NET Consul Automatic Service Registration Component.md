# \.NET Consul Automatic Service Registration Component

## Introduction

A high\-performance microservice registration and health check component based on **\.NET 6\+** and Consul\. This component implements automatic service registration, periodic TTL heartbeat health detection, real\-time health status reporting, and graceful service deregistration, providing stable and reliable service discovery capabilities for distributed microservice systems\.

## Core Features

- **Automatic Service Registration**: Auto register microservice instances to Consul on application startup

- **Dynamic IP Adaptation**: Priority acquisition of K8s Pod IP, compatible with container and physical machine deployment

- **Integrated Health Check**: Link with \.NET native health check system, report real\-time service running status

- **TTL Heartbeat Mechanism**: 5\-second periodic heartbeat reporting, 15\-second TTL health judgment

- **Automatic Offline Cleanup**: Consul automatically clears abnormal offline nodes after 1 minute of critical state

- **Graceful Deregistration**: Active logout from Consul during service shutdown to avoid invalid service nodes

- **Configurable Switch**: Disable Consul registration automatically when the registry address is empty

## Installation \& Usage

### 1\. Configuration

Add the `ConsulConfig` node in `appsettings.json`:

```json
{
  "ConsulConfig": {
    "RegistryAddress": "http://127.0.0.1:8500",
    "ServiceName": "ServiceCenter",
    "ServiceIP": "",
    "ServicePort": 5008
  }
}
```

### 2\. Register Service

Inject the Consul registry component in Program/Startup:

```csharp
builder.Services.AddConsulRegistry(builder.Configuration);
```

## Configuration Explanation

|Field|Description|Rule|
|---|---|---|
|RegistryAddress|Consul server registry address|Empty value will disable the entire Consul registration function|
|ServiceName|Global microservice name|Multiple instances share the same name for service discovery \& load balancing|
|ServiceIP|Service listening IP|Priority: K8s POD\_IP env \&gt; ServiceIP config \&gt; 127\.0\.0\.1 fallback|
|ServicePort|Service listening port|Default: 5008|

## Working Mechanism

### 1\. Startup Registration

After the application starts, the background service automatically generates a unique service ID \(service name \+ machine name\), completes service information registration to Consul, and initializes the TTL health check rule\.

### 2\. Heartbeat \& Health Detection

- Trigger a full application health check every **5 seconds**

- Report `PassTTL` to Consul if all internal checks pass \(service healthy\)

- Report `FailTTL` with abnormal reasons if component exceptions are detected \(service critical\)

- Consul marks the instance as unhealthy if no heartbeat within **15 seconds**

- Consul automatically deletes the abnormal node after **1 minute** of critical state

### 3\. Graceful Shutdown

When the application or K8s Pod stops, the component actively deregisters the current service instance from Consul to ensure the accuracy of the service discovery list and eliminate invalid nodes\.

## Technical Advantages

- Adopts \.NET 6\+ native `BackgroundService` \+ `PeriodicTimer`, avoiding heartbeat overlap and supporting native lifecycle cancellation

- Dock with \.NET official health check system, unified and comprehensive service status detection

- Perfectly compatible with K8s container orchestration, automatically adapt to Pod dynamic IP

- Lightweight and non\-intrusive, enable/disable functions through simple configuration

- Complete exception handling and log output, convenient for online troubleshooting

## Compatibility

- \.NET Version: \.NET 6 / \.NET 7 / \.NET 8\+

- Deployment Environment: Physical machine, Docker, Kubernetes

- Consul Version: 1\.9\+

