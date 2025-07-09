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

A foundational package for building Kontent.ai .NET SDKs with modern resilience, dependency injection, and HTTP client patterns.

## 🎯 **Purpose**

This package provides **core abstractions and utilities** for SDK authors to build specific Kontent.ai SDKs (Delivery, Management, etc.). It is **not intended for direct use by end users** but rather as a foundation for other SDK packages.

## ✨ **What's Included**

- 🏗️ **Base Architecture**: `ClientOptions`, `IActionInvoker`, `IClientFactory`
- 🔄 **Modern Resilience**: Built on Microsoft.Extensions.Http.Resilience (Polly v8)
- 🔌 **DI Integration**: Full support for .NET dependency injection patterns
- 📊 **SDK Tracking**: Automatic tracking headers for analytics
- 🎯 **ActionInvoker**: High-level HTTP operations with automatic serialization
- 🏭 **Factory Patterns**: Support for both direct client registration and factory patterns

## 🚀 **Quick Start for SDK Authors**

### **1. Create Your Client Options**

```csharp
using Kontent.Ai.Core.Configuration;

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
```

### **2. Create Your Client Implementation**

```csharp
using Kontent.Ai.Core.Abstractions;
using Microsoft.Extensions.Options;

public class DeliveryClient
{
    private readonly IActionInvoker _actionInvoker;
    private readonly DeliveryClientOptions _options;

    // Primary constructor for DI scenarios
    public DeliveryClient(IActionInvoker actionInvoker, IOptions<DeliveryClientOptions> options)
        : this(actionInvoker, options.Value)
    {
    }

    // Constructor for factory scenarios  
    public DeliveryClient(IActionInvoker actionInvoker, DeliveryClientOptions options)
    {
        _actionInvoker = actionInvoker ?? throw new ArgumentNullException(nameof(actionInvoker));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    // Constructor for manual instantiation with customer's HttpClient
    public DeliveryClient(HttpClient httpClient, DeliveryClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        _actionInvoker = new ActionInvoker(httpClient, jsonOptions);
    }

    // Your SDK methods
    public async Task<TItem> GetItemAsync<TItem>(string codename, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/{_options.EnvironmentId}/items/{codename}";
        var headers = new Dictionary<string, string>();

        if (_options.UsePreviewApi)
        {
            headers.Add("Authorization", $"Bearer {_options.PreviewApiKey}");
        }

        return await _actionInvoker.GetAsync<TItem>(endpoint, headers, cancellationToken);
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

        return await _actionInvoker.GetAsync<ItemsResponse<TItem>>(endpoint, cancellationToken: cancellationToken);
    }
}
```

### **3. Create Your Factory (Optional)**

```csharp
using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Modules.ClientFactory;

public class DeliveryClientFactory : ClientFactory<DeliveryClient, DeliveryClientOptions>
{
    // Constructor for DI scenarios
    public DeliveryClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<DeliveryClientOptions> optionsMonitor,
        JsonSerializerOptions? jsonOptions = null)
        : base(httpClientFactory, optionsMonitor, jsonOptions)
    {
    }

    // Constructor for standalone scenarios
    public DeliveryClientFactory() : base()
    {
    }

    protected override DeliveryClient CreateClientInstance(IActionInvoker actionInvoker, DeliveryClientOptions options)
    {
        return new DeliveryClient(actionInvoker, options);
    }

    protected override void ConfigureStandaloneHttpClient(HttpClient httpClient, DeliveryClientOptions options)
    {
        base.ConfigureStandaloneHttpClient(httpClient, options);
        
        // Add Delivery-specific headers
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        if (options.UsePreviewApi)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.PreviewApiKey}");
        }
    }
}
```

### **4. Create Your Registration Extensions**

```csharp
using Kontent.Ai.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Delivery SDK with simple configuration.
    /// </summary>
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        DeliveryClientOptions options)
    {
        // Register options
        services.AddSingleton(options);

        // Register HttpClient with all options applied automatically
        services.AddBaseHttpClient(options, client =>
        {
            // Add Delivery-specific configuration
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            if (options.UsePreviewApi)
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.PreviewApiKey}");
            }
        })
        .AddActionInvoker();

        // Register the client
        services.AddClient<DeliveryClient>();

        return services;
    }

    /// <summary>
    /// Registers the Delivery SDK with configuration from appsettings.json.
    /// </summary>
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DeliveryClientOptions>(configuration);

        var options = configuration.Get<DeliveryClientOptions>()!
        return services.AddKontentDelivery(options);
    }

    /// <summary>
    /// Registers the Delivery SDK with factory pattern for multiple configurations.
    /// </summary>
    public static IServiceCollection AddKontentDeliveryFactory(
        this IServiceCollection services,
        IDictionary<string, IConfiguration> configurations)
    {
        // Register multiple named configurations
        foreach (var (name, config) in configurations)
        {
            services.Configure<DeliveryClientOptions>(name, config);
        }

        // Register named HttpClients for each configuration
        foreach (var (name, config) in configurations)
        {
            var options = config.Get<DeliveryClientOptions>()!;
            services.AddBaseHttpClient($"delivery-{name}", options, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                
                if (options.UsePreviewApi)
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.PreviewApiKey}");
                }
            })
            .AddActionInvoker();
        }

        // Register the factory
        services.AddClientFactory<DeliveryClientFactory, DeliveryClient, DeliveryClientOptions>();

        return services;
    }

    /// <summary>
    /// Advanced registration with custom resilience policies.
    /// </summary>
    public static IServiceCollection AddKontentDeliveryWithCustomResilience(
        this IServiceCollection services,
        DeliveryClientOptions options,
        Action<ResiliencePipelineBuilder<HttpResponseMessage>>? configureResilience = null)
    {
        services.AddSingleton(options);

        var builder = services.AddBaseHttpClient(options, client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }, enableDefaultResilience: false); // Disable default resilience

        if (configureResilience != null)
        {
            builder.AddCustomResilienceHandler("delivery-resilience", configureResilience);
        }

        builder.AddActionInvoker();
        services.AddClient<DeliveryClient>();

        return services;
    }
}
```

## 📋 **Core Abstractions**

### **ClientOptions**

Base class for all SDK-specific options. Automatically applied properties:

- **`HttpClientName`**: Used for IHttpClientFactory registration
- **`BaseUrl`**: Sets HttpClient.BaseAddress  
- **`RequestTimeout`**: Sets HttpClient.Timeout
- **`MaxRetryAttempts`**: Configures retry policy
- **`EnvironmentId`**: Your business logic (validated automatically)
- **`ApiKey`**: Manual configuration required (SDK-specific)

### **IActionInvoker**

High-level HTTP operations with automatic serialization:

```csharp
public interface IActionInvoker
{
    Task<TResponse> GetAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<TResponse> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<TResponse> DeleteAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task PostAsync<TRequest>(string endpoint, TRequest payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<TResponse> UploadFileAsync<TResponse>(string endpoint, Stream fileStream, string fileName, string contentType, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}
```

### **IClientFactory<TClient, TOptions>**

Factory pattern for multiple client configurations:

```csharp
public interface IClientFactory<TClient, TOptions> where TOptions : ClientOptions
{
    TClient CreateClient(string name);           // Named configuration
    TClient CreateClient();                      // Default configuration  
    TClient CreateClient(TOptions options);      // Standalone options
    TClient CreateClient(TOptions options, HttpClient httpClient); // Custom HttpClient
}
```

## 🔧 **Registration Patterns**

### **Simple Registration (Single Configuration)**

```csharp
// Program.cs
var deliveryOptions = new DeliveryClientOptions
{
    EnvironmentId = "your-env-id",
    BaseUrl = "https://deliver.kontent.ai/",
    HttpClientName = "delivery-client",
    MaxRetryAttempts = 3,
    RequestTimeout = TimeSpan.FromSeconds(30)
};

builder.Services.AddKontentDelivery(deliveryOptions);

// Usage
public class MyController(DeliveryClient deliveryClient)
{
    public async Task<IActionResult> GetItem(string codename)
    {
        var item = await deliveryClient.GetItemAsync<dynamic>(codename);
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
      "HttpClientName": "delivery-client",
      "MaxRetryAttempts": 3,
      "RequestTimeout": "00:00:30",
      "UsePreviewApi": false
    }
  }
}

// Program.cs
builder.Services.AddKontentDelivery(
    builder.Configuration.GetSection("Kontent:Delivery"));

// Usage is the same as above
```

### **Factory Pattern (Multiple Configurations)**

```csharp
// Program.cs
var configurations = new Dictionary<string, IConfiguration>
{
    ["production"] = builder.Configuration.GetSection("Kontent:Delivery:Production"),
    ["staging"] = builder.Configuration.GetSection("Kontent:Delivery:Staging"),
    ["development"] = builder.Configuration.GetSection("Kontent:Delivery:Development")
};

builder.Services.AddKontentDeliveryFactory(configurations);

// Usage with factory
public class MyService(IClientFactory<DeliveryClient, DeliveryClientOptions> factory)
{
    public async Task<dynamic> GetItemFromEnvironment(string environment, string codename)
    {
        using var client = factory.CreateClient(environment);
        return await client.GetItemAsync<dynamic>(codename);
    }

    public async Task<dynamic> GetItemWithCustomOptions(string codename)
    {
        var customOptions = new DeliveryClientOptions
        {
            EnvironmentId = "different-env-id",
            BaseUrl = "https://deliver.kontent.ai/"
        };

        using var client = factory.CreateClient(customOptions);
        return await client.GetItemAsync<dynamic>(codename);
    }
}
```

### **Custom Resilience Configuration**

```csharp
builder.Services.AddKontentDeliveryWithCustomResilience(
    deliveryOptions,
    pipeline =>
    {
        pipeline
            .AddRetry(new()
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddCircuitBreaker(new()
            {
                FailureRatio = 0.1,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(60)
            })
            .AddTimeout(TimeSpan.FromSeconds(45));
    });
```

## 🏭 **Available Extension Methods**

### **HttpClient Registration**
- `AddBaseHttpClient(string name, ...)` - Basic HttpClient with resilience
- `AddBaseHttpClient(TOptions options, ...)` - Uses all ClientOptions properties
- `AddCustomResilienceHandler(...)` - Custom resilience pipeline

### **ActionInvoker Registration**
- `AddActionInvoker(...)` - Registers IActionInvoker for HttpClient

### **Client Registration** 
- `AddClient<TClient>(...)` - Direct client registration
- `AddClientFactory<TFactory, TClient, TOptions>(...)` - Factory registration
- `AddClientWithOptions<TClient, TOptions>(...)` - All-in-one convenience method

### **Options Registration**
- `services.Configure<TOptions>(configuration)` - Single configuration  
- `services.Configure<TOptions>(name, configuration)` - Named configuration

## 🛡️ **Resilience Policies**

The default resilience configuration uses Microsoft.Extensions.Http.Resilience:

- **Retry Policy**: Exponential backoff with jitter, respects Retry-After headers
- **HTTP Status Codes Handled**: 500+, 408, 429, and transient network errors
- **Customizable**: Override with `AddCustomResilienceHandler()` or set `enableDefaultResilience: false`

Default settings are conservative (2 retry attempts, 2-second base delay) but can be customized via `ClientOptions.MaxRetryAttempts`.

## 🎯 **Best Practices for SDK Authors**

1. **Inherit from ClientOptions**: Always extend `ClientOptions` for your SDK-specific configuration
2. **Use ActionInvoker**: Don't inject HttpClient directly into your client classes
3. **Support Multiple Patterns**: Provide both simple registration and factory patterns
4. **Validate Options**: Override `ClientOptions.Validate()` for your specific requirements
5. **Handle Authentication**: Configure auth headers in your registration extensions, not in core options
6. **Document Examples**: Provide clear examples for all registration patterns
7. **Test All Patterns**: Ensure both DI injection and manual instantiation work correctly

## 📚 **Complete Example: Management SDK**

```csharp
// ManagementClientOptions.cs
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

// ManagementClient.cs  
public class ManagementClient
{
    private readonly IActionInvoker _actionInvoker;
    private readonly ManagementClientOptions _options;

    public ManagementClient(IActionInvoker actionInvoker, ManagementClientOptions options)
    {
        _actionInvoker = actionInvoker;
        _options = options;
        _options.Validate();
    }

    public async Task<ContentItem> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/projects/{_options.EnvironmentId}/items";
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_options.ManagementApiKey}",
            ["Content-Type"] = "application/json"
        };

        return await _actionInvoker.PostAsync<T, ContentItem>(endpoint, item, headers, cancellationToken);
    }
}

// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKontentManagement(
        this IServiceCollection services,
        ManagementClientOptions options)
    {
        services.AddSingleton(options);

        services.AddBaseHttpClient(options, client =>
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ManagementApiKey}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddActionInvoker();

        services.AddClient<ManagementClient>();

        return services;
    }
}
```

## 🤝 **Contributing**

This package is the foundation for all Kontent.ai .NET SDKs. When contributing:

1. **Maintain backward compatibility** with existing SDKs
2. **Follow .NET 8 best practices** (file-scoped namespaces, primary constructors, etc.)
3. **Test with multiple SDK scenarios** to ensure flexibility
4. **Update this README** with any new patterns or breaking changes

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
