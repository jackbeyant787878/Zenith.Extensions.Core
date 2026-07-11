# Zenith\.Extensions Solution

## Overview

**Zenith\.Extensions** is a high\-quality, production\-ready \.NET infrastructure component suite, fully open\-source and published on **NuGet\.org**\. All libraries are designed for modern \.NET microservice architectures, providing encapsulated, high\-availability, thread\-safe middleware capabilities to eliminate repetitive infrastructure coding and unify project technical specifications\.

Every sub\-project maintains independent NuGet packages, standalone documentation, complete DI integration examples and production\-grade fault\-tolerant strategies, supporting global developer installation and usage\.

## 📦 Included NuGet Packages

### 1\. Zenith\.Extensions\.Redis

Lightweight and high\-performance Redis operation library based on **StackExchange\.Redis**\. Fully encapsulates String, Set, Hash common data structures, supports generic auto serialization, sync/async dual APIs and null safety processing\. Simplifies Redis cache development for \.NET applications\.

### 2\. Zenith\.Extensions\.RabbitMQ

Production\-level RabbitMQ message queue SDK based on the latest **RabbitMQ\.Client v7\.x full\-async API**\. Implements persistent connection auto\-reconnection, publisher confirm reliable delivery, dead\-letter \& delay queue mechanism\. Supports Direct / Fanout / Topic / Headers four exchange routing modes, with thread\-safe channel management and unroutable message monitoring\.

### 3\. Zenith\.Extensions\.Consul

Standardized Consul service registration \& discovery extension component\. Provides automatic service health check, dynamic node query and graceful offline capabilities, perfectly adapted to \.NET microservice service governance scenarios\.

### 4\. Zenith\.Extensions\.Elasticsearch

High\-availability Elasticsearch logging component built on **Elastic\.Clients\.Elasticsearch 9\.x**\. Features a custom rolling\-window **circuit breaker**, singleton client anti\-socket\-exhaustion design, environment variable dynamic configuration, and automatic index normalization\. Effectively prevents service avalanche under high\-concurrency log writing scenarios\.

## ✨ Unified Technical Features

- **Modern \.NET Friendly**: Fully compatible with all \.NET 6\+ modern runtime versions, supporting the native async/await programming model

- **DI Native Support**: All components support ASP\.NET Core dependency injection, conforming to \.NET official design specifications

- **Production Fault Tolerance**: Built\-in retry, circuit breaker, connection recovery, thread\-safe control for enterprise\-level stability

- **Engineering Standardization**: Independent English README, complete usage examples, release notes and best practices for each package

- **Zero Intrusive**: Lightweight encapsulation, no redundant dependencies, easy access and low transformation cost

## 🚀 Usage

Each module is independently released to NuGet\.org\. Developers can install any single component via `dotnet add package` according to business requirements, without introducing redundant modules\.

## 📄 License

All Zenith\.Extensions open\-source libraries are free for personal, open\-source and commercial projects\. Welcome to use, learn and expand secondary functions\.

> （注：部分内容可能由 AI 生成）
