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
![.NET](https://img.shields.io/badge/Build-Passing-green.svg)

A foundational package for building Kontent.ai .NET SDKs with modern resilience, dependency injection, and HTTP client patterns using **factory delegates** instead of complex inheritance.

## 🎯 **Purpose**

This package provides **core abstractions and utilities** for SDK authors to build specific Kontent.ai SDKs (Delivery, Management, etc.). It is **not intended for direct use by end users** but rather as a foundation for other SDK packages.

## ✨ **What's Included**

- 🏗️ **Factory Delegates**: Simple `Func<TOptions, HttpClient, TClient>` with direct HttpClient access
- 🔄 **Default Resilience**: Built-in retry, circuit breaker, and timeout policies via Microsoft.Extensions.Http.Resilience
- 🔌 **DI Integration**: Full support for .NET dependency injection patterns with both simple and factory registration
- 📊 **Automatic Tracking**: SDK and source tracking headers (X-KC-SDKID, X-KC-SOURCE)
- 🎯 **Authentication**: Automatic Bearer token handling via `AuthenticationHandler`
- 📈 **Telemetry**: Pluggable API usage monitoring via `IApiUsageListener`
- ⚙️ **Configuration**: Strongly-typed options with clean separation of concerns

## 🚀 **Quick Start for SDK Authors**

### **1. Create Your Client Options**

```csharp
using Kontent.Ai.Core.Configuration;

// For simple single-client scenarios
public class DeliveryClientOptions : ClientOptions
{
    /// <summary>
    /// Whether to use the Preview API for unpublished content.
    /// </summary>
    public bool UsePreviewApi { get; set; } = false;

    /// <summary>
    /// Preview API key (required when UsePreviewApi is true).
    /// </summary>
    public string? PreviewApiKey { get; set; }

    /// <summary>
    /// Whether to include total count in listing responses.
    /// </summary>
    public bool IncludeTotalCount { get; set; } = false;

    public override void Validate()
    {
        base.Validate(); // Validates EnvironmentId, BaseUrl, etc.

        if (UsePreviewApi && string.IsNullOrEmpty(PreviewApiKey))
        {
            throw new ArgumentException("Preview API key is required when UsePreviewApi is true.", nameof(PreviewApiKey));
        }
    }
}

// For multiple named client scenarios (factory pattern)
public class DeliveryNamedClientOptions : NamedClientOptions
{
    public bool UsePreviewApi { get; set; } = false;
    public string? PreviewApiKey { get; set; }
    public bool IncludeTotalCount { get; set; } = false;

    public override void Validate()
    {
        base.Validate(); // Validates EnvironmentId, BaseUrl, HttpClientName, etc.

        if (UsePreviewApi && string.IsNullOrEmpty(PreviewApiKey))
        {
            throw new ArgumentException("Preview API key is required when UsePreviewApi is true.", nameof(PreviewApiKey));
        }
    }
}
```

### **2. Create Your Client Implementation**

```csharp
using Kontent.Ai.Core.Abstractions;
using Microsoft.Extensions.Options;

public class DeliveryClient
{
    private readonly HttpClient _httpClient;
    private readonly DeliveryClientOptions _options;

    // Primary constructor for factory scenarios (gets HttpClient from factory)
    public DeliveryClient(DeliveryClientOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options.Validate();
    }

    // Constructor for DI scenarios (uses IHttpClientFactory internally)
    public DeliveryClient(IOptions<DeliveryClientOptions> options, IHttpClientFactory httpClientFactory)
        : this(options.Value, httpClientFactory.CreateClient("kontent-ai-deliveryclient"))
    {
    }

    // Your SDK methods using HttpClient directly
    public async Task<TItem> GetItemAsync<TItem>(string codename, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/{_options.EnvironmentId}/items/{codename}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        
        if (_options.UsePreviewApi)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.PreviewApiKey);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TItem>(json, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        })!;
    }

    public async Task<ItemsResponse<TItem>> GetItemsAsync<TItem>(
        string? filter = null, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"/{_options.EnvironmentId}/items";
        if (!string.IsNullOrEmpty(filter))
        {
            endpoint += $"?{filter}";
        }

        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ItemsResponse<TItem>>(json, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        })!;
    }
}
```

### **3. Create Your Registration Extensions**

```csharp
using Kontent.Ai.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Delivery SDK with simple configuration (singleton pattern).
    /// Most users will use this method for single-environment scenarios.
    /// </summary>
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        DeliveryClientOptions options)
    {
        // Register core services first (handlers, tracking, telemetry)
        services.AddCoreServices();
        
        // Register client using simple pattern with automatic HttpClient naming
        return services.AddClient<DeliveryClient, DeliveryClientOptions>(
            options,
            (opts, httpClient) => new DeliveryClient(opts, httpClient), // Factory delegate with HttpClient
            httpClientBuilder =>
            {
                // Add delivery-specific configuration
                httpClientBuilder.ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });
            });
    }

    /// <summary>
    /// Registers the Delivery SDK with configuration delegate.
    /// </summary>
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        Action<DeliveryClientOptions> configureOptions,
        Action<IHttpClientBuilder>? configureHttpClient = null)
    {
        services.AddCoreServices();
        
        return services.AddClient<DeliveryClient, DeliveryClientOptions>(
            configureOptions,
            (opts, httpClient) => new DeliveryClient(opts, httpClient),
            configureHttpClient);
    }

    /// <summary>
    /// Registers the Delivery SDK with factory pattern for multiple configurations.
    /// Advanced users can configure multiple environments (prod, staging, dev).
    /// </summary>
    public static IServiceCollection AddKontentDeliveryFactory(
        this IServiceCollection services,
        Action<ClientFactoryBuilder<DeliveryClient, DeliveryNamedClientOptions>> configureFactory)
    {
        services.AddCoreServices();
        
        return services.AddMultipleClientFactory<DeliveryClient, DeliveryNamedClientOptions>(
            (opts, httpClient) => new DeliveryClient(opts, httpClient), // Factory delegate with HttpClient
            configureFactory);
    }
}
```

## 📋 **Core Abstractions**

### **ClientOptions**

Base class for all SDK-specific options with simple resilience control:

```csharp
public abstract class ClientOptions
{
    public required string EnvironmentId { get; set; }           // Your Kontent.ai environment
    public string? ApiKey { get; set; }                          // Handled by AuthenticationHandler
    public required string BaseUrl { get; set; }                // API base URL
    
    // Simple resilience control - just enable/disable defaults
    public bool EnableDefaultResilience { get; set; } = true;   // Default: enabled with sensible settings
}
```

### **NamedClientOptions**

Extended class for factory scenarios requiring HttpClient naming:

```csharp
public abstract class NamedClientOptions : ClientOptions
{
    public required string HttpClientName { get; set; }         // HttpClient name for factory scenarios
}
```

### **IMultipleClientFactory<TClient, TOptions>**

Factory interface for multi-client scenarios:

```csharp
public interface IMultipleClientFactory<TClient, TOptions> where TOptions : NamedClientOptions
{
    TClient CreateClient(string name);                           // Named configuration
    IEnumerable<string> GetRegisteredClientNames();             // List all names
    bool IsClientRegistered(string name);                       // Check if exists
}
```

### **IApiUsageListener**

Interface for telemetry and monitoring:

```csharp
public interface IApiUsageListener
{
    Task OnRequestStartAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    Task OnRequestEndAsync(HttpRequestMessage request, HttpResponseMessage? response, 
        Exception? exception, TimeSpan elapsed, CancellationToken cancellationToken = default);
}
```

## 🔧 **Registration Patterns**

### **Simple Registration (Singleton Pattern)**

```csharp
// Program.cs - Most common usage
var deliveryOptions = new DeliveryClientOptions
{
    EnvironmentId = "your-env-id",
    BaseUrl = "https://deliver.kontent.ai/",
    EnableDefaultResilience = true,    // Simple on/off switch for default policies
};

builder.Services.AddKontentDelivery(deliveryOptions);

// Usage - Direct injection
public class MyController(DeliveryClient deliveryClient)
{
    public async Task<IActionResult> GetItem(string codename)
    {
        var item = await deliveryClient.GetItemAsync<ContentItem>(codename);
        return Ok(item);
    }
}
```

### **Configuration-based Registration**

```csharp
// appsettings.json
{
  "Kontent": {
    "Delivery": {
      "EnvironmentId": "your-env-id",
      "BaseUrl": "https://deliver.kontent.ai/",
      "EnableDefaultResilience": true,
      "UsePreviewApi": false
    }
  }
}

// Program.cs
builder.Services.Configure<DeliveryClientOptions>(
    builder.Configuration.GetSection("Kontent:Delivery"));

var options = builder.Configuration.GetSection("Kontent:Delivery").Get<DeliveryClientOptions>()!;
builder.Services.AddKontentDelivery(options);
```

### **Multiple Clients (Factory Pattern)**

```csharp
// Program.cs - Multiple environments
builder.Services.AddKontentDeliveryFactory(factory =>
{
    factory.AddClient("production", opts =>
    {
        opts.EnvironmentId = "prod-env-id";
        opts.BaseUrl = "https://deliver.kontent.ai/";
        opts.ApiKey = "prod-api-key";
        opts.HttpClientName = "kontent-ai-client-production";
        opts.EnableDefaultResilience = true;  // Use defaults for production
    });
    
    factory.AddClient("staging", opts =>
    {
        opts.EnvironmentId = "staging-env-id";
        opts.BaseUrl = "https://deliver.kontent.ai/";
        opts.ApiKey = "staging-api-key";
        opts.HttpClientName = "kontent-ai-client-staging";
        opts.EnableDefaultResilience = true;  // Use defaults for staging
    });
    
    factory.AddClient("development", opts =>
    {
        opts.EnvironmentId = "dev-env-id";
        opts.BaseUrl = "https://deliver.kontent.ai/";
        opts.HttpClientName = "kontent-ai-client-development";
        opts.EnableDefaultResilience = false; // No resilience for development
    }, configureResilience: httpClientBuilder =>
    {
        // Custom resilience for development (if needed)
        httpClientBuilder.AddDefaultResilienceHandler(new ResilienceOptions
        {
            EnableRetry = false,
            EnableCircuitBreaker = false,
            EnableTimeout = true
        });
    });
});

// Usage with factory
public class MyService(IMultipleClientFactory<DeliveryClient, DeliveryNamedClientOptions> factory)
{
    public async Task<ContentItem> GetItemFromEnvironment(string environment, string codename)
    {
        var client = factory.CreateClient(environment);
        return await client.GetItemAsync<ContentItem>(codename);
    }
}
```

### **Custom Resilience Configuration**

```csharp
// Program.cs - Custom resilience for high-traffic scenarios
var deliveryOptions = new DeliveryClientOptions
{
    EnvironmentId = "your-env-id",
    BaseUrl = "https://deliver.kontent.ai/",
    EnableDefaultResilience = false  // Disable defaults to use custom configuration
};

builder.Services.AddKontentDelivery(deliveryOptions, httpClientBuilder =>
{
    // Custom resilience configuration
    httpClientBuilder.AddDefaultResilienceHandler(new ResilienceOptions
    {
        EnableRetry = true,
        EnableCircuitBreaker = true,
        EnableTimeout = true,
        Retry = new RetryOptions
        {
            MaxRetryAttempts = 10,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            UseJitter = true,
            UseExponentialBackoff = true
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.2,
            BreakDuration = TimeSpan.FromMinutes(2)
        },
        Timeout = new TimeoutOptions
        {
            Timeout = TimeSpan.FromMinutes(5)
        }
    });
});
```

## 🏭 **Available Extension Methods**

### **Core Services Registration**
- `AddCoreServices(CoreServicesOptions?)` - Registers handlers, tracking, telemetry, JSON options

### **Client Registration**
- `AddClient<TClient, TOptions>(options, factory, configure?)` - Simple singleton registration with auto-generated HttpClient name
- `AddClient<TClient, TOptions>(configureOptions, factory, configure?)` - Configuration delegate variant
- `AddMultipleClientFactory<TClient, TOptions>(factory, configure)` - Multi-client factory registration

### **Resilience Configuration**
- `AddDefaultResilienceHandler(ResilienceOptions?)` - Apply Kontent.ai-optimized resilience policies

### **Handler Registration**
- `AddRequestHandlers()` - Adds authentication, tracking, and telemetry handlers in correct order
- `AddRequestHandlers<TOptions>()` - Type-safe handler registration for specific options

## 🛡️ **Default Resilience Policies**

When `EnableDefaultResilience = true` (default), your HttpClients automatically get:

### **Retry Policy**
- **MaxRetryAttempts**: 3 attempts
- **Delay**: Exponential backoff starting from 1 second
- **Jitter**: Enabled to prevent thundering herd
- **Handles**: HTTP 5xx, 408 (timeout), 429 (rate limit), network errors

### **Circuit Breaker**
- **FailureRatio**: 50% failures trigger circuit break
- **SamplingDuration**: 30 seconds
- **MinimumThroughput**: 10 requests before evaluation
- **BreakDuration**: 30 seconds

### **Timeout**
- **TotalRequestTimeout**: 30 seconds per request

All default values are optimized for Kontent.ai APIs based on real-world usage patterns.

## 🎯 **Best Practices for SDK Authors**

1. **Use Factory Delegates**: Simple `Func<TOptions, HttpClient, TClient>` with direct HttpClient access
2. **Extend Appropriate Base**: Use `ClientOptions` for simple scenarios, `NamedClientOptions` for factory scenarios
3. **Call AddCoreServices()**: Register core services before your specific SDK services
4. **Support Multiple Patterns**: Provide both simple and factory registration methods
5. **Validate Options**: Override `ClientOptions.Validate()` for your requirements
6. **Use HttpClient Directly**: No need for additional abstractions - direct HttpClient usage is encouraged
7. **Configure Authentication**: Use the built-in `AuthenticationHandler` for Bearer tokens
8. **Keep Configuration Simple**: Use `EnableDefaultResilience` boolean for most users, advanced users can configure custom resilience

## 📚 **Complete Example: Management SDK**

```csharp
// ManagementClientOptions.cs - Simple version
public class ManagementClientOptions : ClientOptions
{
    public string? ManagementApiKey { get; set; }
    public bool UseProductionApi { get; set; } = true;
    
    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrEmpty(ManagementApiKey))
            throw new ArgumentException("Management API key is required.", nameof(ManagementApiKey));
    }
}

// ManagementNamedClientOptions.cs - For factory scenarios
public class ManagementNamedClientOptions : NamedClientOptions
{
    public string? ManagementApiKey { get; set; }
    public bool UseProductionApi { get; set; } = true;
    
    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrEmpty(ManagementApiKey))
            throw new ArgumentException("Management API key is required.", nameof(ManagementApiKey));
    }
}

// ManagementClient.cs  
public class ManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly ManagementClientOptions _options;

    public ManagementClient(ManagementClientOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options.Validate();
    }

    public async Task<ContentItem> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/projects/{_options.EnvironmentId}/items";
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ManagementApiKey);
        request.Headers.Add("Content-Type", "application/json");

        var json = JsonSerializer.Serialize(item, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ContentItem>(responseJson, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        })!;
    }
}

// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKontentManagement(
        this IServiceCollection services,
        ManagementClientOptions options)
    {
        services.AddCoreServices();
        
        return services.AddClient<ManagementClient, ManagementClientOptions>(
            options,
            (opts, httpClient) => new ManagementClient(opts, httpClient),
            httpClientBuilder =>
            {
                // Management API typically needs different retry strategy
                if (!options.EnableDefaultResilience)
                {
                    httpClientBuilder.AddDefaultResilienceHandler(new ResilienceOptions
                    {
                        EnableRetry = true,
                        EnableCircuitBreaker = true,
                        EnableTimeout = true,
                        Retry = new RetryOptions
                        {
                            MaxRetryAttempts = 2, // Fewer retries for write operations
                            BaseDelay = TimeSpan.FromSeconds(3) // Longer delays
                        }
                    });
                }
            });
    }
}
```

## 🔄 **Migration from Legacy Patterns**

If you're migrating from inheritance-based factories to factory delegates:

### **Before (Inheritance)**
```csharp
public class DeliveryClientFactory : ClientFactory<DeliveryClient, DeliveryClientOptions>
{
    protected override DeliveryClient CreateClientInstance(IActionInvoker invoker, DeliveryClientOptions options)
    {
        return new DeliveryClient(invoker, options);
    }
}
```

### **After (Factory Delegates)**
```csharp
services.AddMultipleClientFactory<DeliveryClient, DeliveryNamedClientOptions>(
    (opts, httpClient) => new DeliveryClient(opts, httpClient), // Simple factory delegate with HttpClient
    factory => { /* configure clients */ });
```

### **Key Changes in Factory Delegates**
1. **HttpClient Parameter**: Factory delegates now receive `HttpClient` directly: `Func<TOptions, HttpClient, TClient>`
2. **Options Classes**: Use `NamedClientOptions` for factory scenarios to include `HttpClientName`
3. **Simplified Configuration**: `EnableDefaultResilience` boolean instead of detailed retry configuration in options
4. **Direct HttpClient Usage**: No need for `IActionInvoker` abstraction - use `HttpClient` methods directly

## 🤝 **Contributing**

This package is the foundation for all Kontent.ai .NET SDKs. When contributing:

1. **Maintain backward compatibility** with existing SDKs
2. **Follow .NET 8 best practices** (file-scoped namespaces, primary constructors, etc.)
3. **Test with multiple SDK scenarios** to ensure flexibility
4. **Keep factory delegates simple** - avoid complex inheritance patterns
5. **Document resilience behavior** clearly for SDK authors

## 📄 **License**

MIT License - see [LICENSE.md](LICENSE.md) for details.

<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/kontent-ai/core-sdk-net.svg?style=for-the-badge
[contributors-url]: https://github.com/kontent-ai/core-sdk-net/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/kontent-ai/core-sdk-net.svg?style=for-the-badge
[forks-url]: https://github.com/kontent-ai/core-sdk-net/network/members
[stars-shield]: https://img.shields.io/github/stars/kontent-ai/core-sdk-net.svg?style=for-the-badge
[stars-url]: https://github.com/kontent-ai/core-sdk-net/stargazers
[issues-shield]: https://img.shields.io/github/issues/kontent-ai/core-sdk-net.svg?style=for-the-badge
[issues-url]: https://github.com/kontent-ai/core-sdk-net/issues
[license-shield]: https://img.shields.io/github/license/kontent-ai/core-sdk-net.svg?style=for-the-badge
[license-url]: https://github.com/kontent-ai/core-sdk-net/blob/master/LICENSE.md
[discussion-shield]: https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%2Eai%20Discord&logo=discord&style=for-the-badge
[discussion-url]: https://discord.com/invite/SKCxwPtevJ
