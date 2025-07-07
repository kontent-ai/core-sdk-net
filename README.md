[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

[![Discord][discussion-shield]][discussion-url]

> [!IMPORTANT]  
> 🚧 This package is currently being developed. 🚧

# Kontent.ai Core SDK for .NET

![License](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Build](https://img.shields.io/badge/Build-Passing-green.svg)

A modern, resilient core package for Kontent.ai .NET SDKs using **Microsoft.Extensions.Http.Resilience** and following .NET 8 best practices.

## ✨ **Features**

- 🔄 **Modern Resilience**: Built on Microsoft.Extensions.Http.Resilience (Polly v8)
- 🎯 **ActionInvoker Pattern**: High-level HTTP operations with automatic serialization
- 🔧 **Configurable Pipelines**: Separate optimizations for read vs. write operations
- 📊 **SDK Tracking**: Automatic tracking headers for analytics
- 🏗️ **Modern .NET**: Uses file-scoped namespaces, primary constructors, and global usings
- 🔌 **IHttpClientFactory**: Full integration with .NET's HTTP client factory

## 🚀 **Quick Start**

### **For Delivery SDK (Read-Heavy Workloads)**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register resilient HTTP client optimized for read operations
builder.Services.AddKontentDeliveryHttpClient("delivery-client", client =>
{
    client.BaseAddress = new Uri("https://deliver.kontent.ai/");
    client.DefaultRequestHeaders.Add("X-KC-WAIT-FOR-LOADING-NEW-CONTENT", "true");
})
.AddActionInvoker(); // Add high-level HTTP operations

var app = builder.Build();

// Your service
public class WeatherService(IActionInvoker actionInvoker)
{
    public async Task<WeatherData[]> GetWeatherAsync(CancellationToken cancellationToken = default)
    {
        return await actionInvoker.GetAsync<WeatherData[]>("/weather", cancellationToken: cancellationToken);
    }
}
```

### **For Management SDK (Write-Heavy Workloads)**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register resilient HTTP client optimized for write operations
builder.Services.AddKontentManagementHttpClient("management-client", client =>
{
    client.BaseAddress = new Uri("https://manage.kontent.ai/");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_API_KEY");
})
.AddActionInvoker(options =>
{
    // Customize JSON serialization
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var app = builder.Build();

// Your service
public class ContentService(IActionInvoker actionInvoker)
{
    public async Task<ContentItem> CreateContentAsync(CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        return await actionInvoker.PostAsync<CreateContentRequest, ContentItem>("/items", request, cancellationToken: cancellationToken);
    }
    
    public async Task DeleteContentAsync(string itemId, CancellationToken cancellationToken = default)
    {
        await actionInvoker.DeleteAsync($"/items/{itemId}", cancellationToken: cancellationToken);
    }
}
```

## 🔧 **Advanced Configuration**

### **Custom Resilience Pipeline**

```csharp
builder.Services.AddKontentDeliveryHttpClient("custom-client")
    .AddCustomResilienceHandler("custom-pipeline", pipeline =>
    {
        pipeline
            .AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.1,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .AddTimeout(TimeSpan.FromSeconds(10));
    });
```

### **Options Configuration**

```csharp
// appsettings.json
{
  "KontentDelivery": {
    "EnvironmentId": "your-environment-id",
    "BaseUrl": "https://deliver.kontent.ai/"
  }
}

// Program.cs
builder.Services.AddClientOptions<DeliveryOptions>(
    builder.Configuration.GetSection("KontentDelivery"));
```

## 📋 **IActionInvoker API**

The ActionInvoker provides high-level HTTP operations:

```csharp
public interface IActionInvoker
{
    // GET operations
    Task<TResponse> GetAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // POST operations
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task PostAsync<TRequest>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // PUT operations  
    Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // PATCH operations
    Task<TResponse> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // DELETE operations
    Task<TResponse> DeleteAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // File uploads
    Task<TResponse> UploadFileAsync<TResponse>(string endpoint, Stream fileStream, string fileName, string contentType, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    
    // Raw HTTP operations
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
```

## 🛡️ **Resilience Policies**

### **Delivery Client Defaults (Read-Optimized)**
- **Retry**: 3 attempts, exponential backoff with jitter
- **Timeout**: 10s per attempt, 30s total
- **Circuit Breaker**: 10% failure ratio, 30s sampling
- **Rate Limiting**: 1,000 concurrent requests

### **Management Client Defaults (Write-Optimized)**
- **Retry**: 2 attempts, exponential backoff with jitter
- **Timeout**: 30s per attempt, 60s total  
- **Circuit Breaker**: 10% failure ratio, 30s sampling (more sensitive)
- **Rate Limiting**: 1,000 concurrent requests

## 🆚 **Modern vs Legacy Approach**

| Feature | Legacy (Custom Retry) | Modern (Microsoft.Extensions.Http.Resilience) |
|---------|---------------------|------------------------------------------------|
| **Performance** | Custom implementation | Polly v8 (4x better memory usage) |
| **Features** | Basic retry + backoff | Rate limiting, circuit breaker, hedging |
| **Maintenance** | Custom code to maintain | Microsoft-supported |
| **Integration** | Manual DI setup | Native IHttpClientFactory integration |
| **Telemetry** | Custom | Built-in metrics & logging |
| **Configuration** | Code-based | Configuration-based with reloading |

## 🔧 **Migration from Legacy**

### **From BaseClient Pattern**
```csharp
// Before
public class DeliveryClient : BaseClient
{
    protected override Assembly? SdkAssembly => typeof(DeliveryClient).Assembly;
}

// After  
public class DeliveryClient(IActionInvoker actionInvoker)
{
    private readonly IActionInvoker _actionInvoker = actionInvoker;
    
    public async Task<T> GetItemAsync<T>(string codename) =>
        await _actionInvoker.GetAsync<T>($"/items/{codename}");
}
```

### **From Custom Retry Policies**
```csharp
// Before
services.AddSingleton<IRetryPolicyProvider, DefaultRetryPolicyProvider>();

// After - handled automatically by AddKontentDeliveryHttpClient
```

## 🌟 **Benefits of Modern Approach**

1. **Performance**: Polly v8 offers 4x better memory usage and better performance
2. **Feature Rich**: Circuit breakers, rate limiting, hedging, and advanced retry policies
3. **Microsoft Supported**: Official Microsoft package with long-term support
4. **Configuration-Driven**: Dynamic reloading of resilience policies
5. **Industry Standard**: Follows established patterns for cloud-native applications
6. **Better Telemetry**: Built-in metrics and logging integration

## 📊 **Status Codes Handled**

The resilience policies automatically handle:
- **500+** (Server errors)
- **408** (Request Timeout)  
- **429** (Too Many Requests with Retry-After support)
- **Transient network errors** (connection failures, timeouts)

## 🏗️ **For SDK Authors**

```csharp
// Register your SDK's HTTP client
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyKontentSdk(this IServiceCollection services, MyOptions options)
    {
        services.AddKontentDeliveryHttpClient("my-sdk", client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            if (!string.IsNullOrEmpty(options.ApiKey))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
        })
        .AddActionInvoker();
        
        services.AddScoped<IMyKontentClient, MyKontentClient>();
        return services;
    }
}

// Your client implementation
public class MyKontentClient(IActionInvoker actionInvoker) : IMyKontentClient
{
    public async Task<ContentItem[]> GetItemsAsync() =>
        await actionInvoker.GetAsync<ContentItem[]>("/items");
}
```

## 📚 **Documentation**

- [Microsoft.Extensions.Http.Resilience Docs](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Polly Documentation](https://www.pollydocs.org/)
- [.NET HttpClientFactory](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)

## 🤝 **Contributing**

This package consolidates common functionality to reduce duplication across Kontent.ai SDKs. When adding new features, consider:

1. **Backward compatibility** with existing SDKs
2. **Performance** implications of shared code
3. **Modern .NET standards** alignment
4. **Testing** across multiple SDK scenarios

## 📄 **License**

MIT License - see [LICENSE.md](LICENSE.md) for details.

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://github.com/kontent-ai/Home/wiki/Checklist-for-publishing-a-new-OS-project#badges-->
[contributors-shield]: https://img.shields.io/github/contributors/kontent-ai/core-sdk-net.svg?style=for-the-badge
[contributors-url]: https://github.com/kontent-ai/repo-template/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/kontent-ai/core-sdk-net.svg?style=for-the-badge
[forks-url]: https://github.com/kontent-ai/repo-template/network/members
[stars-shield]: https://img.shields.io/github/stars/kontent-ai/core-sdk-net.svg?style=for-the-badge
[stars-url]: https://github.com/kontent-ai/repo-template/stargazers
[issues-shield]: https://img.shields.io/github/issues/kontent-ai/core-sdk-net.svg?style=for-the-badge
[issues-url]:https://github.com/kontent-ai/repo-template/issues
[license-shield]: https://img.shields.io/github/license/kontent-ai/core-sdk-net.svg?style=for-the-badge
[license-url]:https://github.com/kontent-ai/repo-template/blob/master/LICENSE.md
[discussion-shield]: https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%2Eai%20Discord&logo=discord&style=for-the-badge
[discussion-url]: https://discord.com/invite/SKCxwPtevJ
