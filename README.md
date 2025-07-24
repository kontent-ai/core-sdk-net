[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

[![Discord][discussion-shield]][discussion-url]

> [!IMPORTANT]  
> 🚧 This package showcases the proposed architecture for the upcoming Kontent.ai .NET SDKs rewrite. 🚧
> 
> The Readme is AI generated for the review purposes. Simplified readme will be provided for the release.

# Kontent.ai Core SDK for .NET

![License](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Refit](https://img.shields.io/badge/HTTP-Refit-blue.svg)

A foundational package for building Kontent.ai .NET SDKs with **Refit at the core**, providing modern resilience, dependency injection, and declarative HTTP API patterns.

## 🎯 **Architecture Vision for rewritten SDKs**

This package demonstrates the **proposed architecture change** for the upcoming Kontent.ai .NET SDK rewrite, moving from traditional HTTP client patterns to a **Refit-centered approach** with automatic infrastructure configuration.

### **Key Architectural Decisions**

- **🔥 Refit as HTTP Foundation**: All HTTP communication through declarative Refit interfaces
- **⚡ Zero Boilerplate**: Automatic configuration of authentication, tracking, resilience, and JSON serialization  
- **🏗️ Composable Design**: Clean separation between HTTP layer (Refit) and business logic
- **🔧 Smart Defaults**: Production-ready configuration with minimal setup required
- **📦 Infrastructure as a Service**: Core package handles all cross-cutting concerns

## ✨ **What This Core Package Provides**

- **🌐 Refit Integration**: Seamless registration and configuration of Refit clients
- **🔐 Authentication**: Automatic Bearer token injection via `AuthenticationHandler<TOptions>`
- **📊 Tracking**: SDK and source tracking headers (X-KC-SDKID, X-KC-SOURCE) via `TrackingHandler`
- **📈 Telemetry**: Pluggable API monitoring via `IApiUsageListener` and `TelemetryHandler`
- **🛡️ Resilience**: Built-in retry, circuit breaker, and timeout policies (can be disabled via `EnableResilience = false`)
- **⚙️ JSON Configuration**: Optimized JSON serialization settings for Kontent.ai APIs
- **🔌 DI Integration**: Full .NET dependency injection support with strongly-typed options

## 🚀 **SDK Author Guide: Building on Refit**

### **1. Define Your Refit API Interface**

The new architecture starts with **declarative Refit interfaces** instead of manual HTTP client code:

```csharp
using Refit;

// Delivery API Example
public interface IDeliveryApi
{
    [Get("/{environmentId}/items")]
    Task<ItemsResponse<TItem>> GetItemsAsync<TItem>(
        string environmentId,
        [Query] string? elements = null,
        [Query] string? filter = null,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);

    [Get("/{environmentId}/items/{codename}")]
    Task<ItemResponse<TItem>> GetItemAsync<TItem>(
        string environmentId, 
        string codename,
        [Query] string? elements = null,
        CancellationToken cancellationToken = default);

    [Get("/{environmentId}/types")]
    Task<TypesResponse> GetTypesAsync(
        string environmentId,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);
}

// Management API Example
public interface IManagementApi
{
    [Get("/v2/projects/{environmentId}/items")]
    Task<ItemsResponse> GetItemsAsync(
        string environmentId,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);

    [Post("/v2/projects/{environmentId}/items")]
    Task<ItemResponse> CreateItemAsync(
        string environmentId,
        [Body] CreateItemRequest request,
        CancellationToken cancellationToken = default);

    [Put("/v2/projects/{environmentId}/items/{itemId}")]
    Task<ItemResponse> UpdateItemAsync(
        string environmentId,
        string itemId,
        [Body] UpdateItemRequest request,
        CancellationToken cancellationToken = default);

    [Delete("/v2/projects/{environmentId}/items/{itemId}")]
    Task DeleteItemAsync(
        string environmentId,
        string itemId,
        CancellationToken cancellationToken = default);
}
```

### **2. Create Client Options**

Define strongly-typed configuration options that extend the base `ClientOptions`:

```csharp
using Kontent.Ai.Core.Configuration;

// Delivery SDK Options
public record DeliveryClientOptions : ClientOptions
{
    /// <summary>
    /// Production endpoint for delivery API.
    /// </summary>
    public string ProductionEndpoint { get; set; } = "https://deliver.kontent.ai";
    
    /// <summary>
    /// Preview endpoint for delivery API.
    /// </summary>
    public string PreviewEndpoint { get; set; } = "https://preview-deliver.kontent.ai";

    /// <summary>
    /// Whether to use the Preview API for unpublished content.
    /// </summary>
    public bool UsePreviewApi { get; set; } = false;

    /// <summary>
    /// Preview API key (required when UsePreviewApi is true).
    /// </summary>
    public string? PreviewApiKey { get; set; }
    
    /// <summary>
    /// Secure access API key for production endpoint.
    /// </summary>
    public string? SecureAccessApiKey { get; set; }

    /// <summary>
    /// Whether to include total count in listing responses.
    /// </summary>
    public bool IncludeTotalCount { get; set; } = false;
}

// Extension methods for DeliveryClientOptions
public static class DeliveryClientOptionsExtensions
{
    public static string GetBaseUrl(this DeliveryClientOptions options, object? requestContext = null)
    {
        return options.UsePreviewApi ? options.PreviewEndpoint : options.ProductionEndpoint;
    }

    public static string? GetApiKey(this DeliveryClientOptions options, object? requestContext = null)
    {
        return options.UsePreviewApi ? options.PreviewApiKey : options.SecureAccessApiKey;
    }

    public static void Validate(this DeliveryClientOptions options)
    {
        // Call base validation for EnvironmentId
        ((ClientOptions)options).Validate();

        if (options.UsePreviewApi && string.IsNullOrEmpty(options.PreviewApiKey))
        {
            throw new ArgumentException("Preview API key is required when UsePreviewApi is true.", nameof(options.PreviewApiKey));
        }
    }
}

// Management SDK Options  
public record ManagementClientOptions : ClientOptions
{
    /// <summary>
    /// Management API endpoint.
    /// </summary>
    public string ManagementEndpoint { get; set; } = "https://manage.kontent.ai";

    /// <summary>
    /// Management API key for write operations.
    /// </summary>
    public required string ManagementApiKey { get; set; }

    /// <summary>
    /// Whether to use the production Management API endpoint.
    /// </summary>
    public bool UseProductionApi { get; set; } = true;
}

// Extension methods for ManagementClientOptions
public static class ManagementClientOptionsExtensions
{
    public static string GetBaseUrl(this ManagementClientOptions options, object? requestContext = null)
    {
        return options.ManagementEndpoint;
    }

    public static string? GetApiKey(this ManagementClientOptions options, object? requestContext = null)
    {
        return options.ManagementApiKey;
    }

    public static void Validate(this ManagementClientOptions options)
    {
        // Call base validation for EnvironmentId
        ((ClientOptions)options).Validate();
        
        if (string.IsNullOrEmpty(options.ManagementApiKey))
        {
            throw new ArgumentException("Management API key is required.", nameof(options.ManagementApiKey));
        }
    }
}
```

### **3. Create Your Client Implementation**

Build your client around the Refit interface, with **zero HTTP boilerplate**:

```csharp
using Microsoft.Extensions.Options;

// Delivery Client
public class DeliveryClient
{
    private readonly IDeliveryApi _api;
    private readonly DeliveryClientOptions _options;

    public DeliveryClient(IDeliveryApi api, IOptions<DeliveryClientOptions> options)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    // Business logic methods that delegate to Refit interface
    public async Task<TItem[]> GetItemsAsync<TItem>(
        string? filter = null, 
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.GetItemsAsync<TItem>(
            _options.EnvironmentId,
            filter: filter,
            limit: limit,
            cancellationToken: cancellationToken);

        return response.Items;
    }

    public async Task<TItem> GetItemAsync<TItem>(
        string codename, 
        CancellationToken cancellationToken = default)
    {
        var response = await _api.GetItemAsync<TItem>(
            _options.EnvironmentId, 
            codename, 
            cancellationToken: cancellationToken);

        return response.Item;
    }
}

// Management Client - Shows different patterns for write operations
public class ManagementClient
{
    private readonly IManagementApi _api;
    private readonly ManagementClientOptions _options;

    public ManagementClient(IManagementApi api, IOptions<ManagementClientOptions> options)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ContentItem> CreateItemAsync(
        CreateItemRequest request, 
        CancellationToken cancellationToken = default)
    {
        var response = await _api.CreateItemAsync(
            _options.EnvironmentId, 
            request, 
            cancellationToken);

        return response.Item;
    }

    public async Task<ContentItem> UpdateItemAsync(
        string itemId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.UpdateItemAsync(
            _options.EnvironmentId,
            itemId,
            request,
            cancellationToken);

        return response.Item;
    }
}
```

### **4. Register Everything with the Core Package**

The core package handles **all the infrastructure setup** - you just register your Refit interface and client:

```csharp
using Kontent.Ai.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Delivery SDK with the new Refit-based architecture.
    /// </summary>
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        Action<DeliveryClientOptions> configureOptions)
    {
        // Register the Refit interface with all Kontent.ai infrastructure
        services.AddClient<IDeliveryApi, DeliveryClientOptions>(
            configureOptions,
            configureRefitSettings: settings =>
            {
                // Customize Refit settings if needed
                // Default settings are already optimized for Kontent.ai
            },
            configureHttpClient: httpClient =>
            {
                // Add any delivery-specific HTTP configuration
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            });

        // Register the business logic client
        services.AddTransient<DeliveryClient>();

        return services;
    }

    /// <summary>
    /// Registers the Management SDK with write-operation optimized settings.
    /// </summary>
    public static IServiceCollection AddKontentManagement(
        this IServiceCollection services,
        Action<ManagementClientOptions> configureOptions)
    {
        services.AddClient<IManagementApi, ManagementClientOptions>(
            configureOptions,
            configureResilience: (builder, clientOptions) =>
            {
                // Note: Default strategies (retry, timeout, circuit breaker) are already added
                // For write operations, you might want to add additional conservative strategies
                // or customize behavior based on client options
            });

        services.AddTransient<ManagementClient>();
        return services;
    }
}
```

## 🏗️ **Core Architecture Components**

### **AddClient<T, TOptions>() - The Magic Method**

This is the core registration method that wraps Refit with all Kontent.ai infrastructure:

```csharp
// What happens under the hood when you call AddClient:
services.AddClient<IDeliveryApi, DeliveryClientOptions>(configureOptions)
// 1. Registers core services (handlers, telemetry, JSON settings)
// 2. Configures and validates your options  
// 3. Registers Refit client with optimized settings
// 4. Adds authentication handler (Bearer token from options.ApiKey)
// 5. Adds tracking handler (X-KC-SDKID, X-KC-SOURCE headers)
// 6. Adds telemetry handler (IApiUsageListener notifications)
// 7. Configures resilience policies (retry, circuit breaker, timeout)
// 8. Sets up JSON serialization (camelCase, null handling, etc.)
```

### **Automatic Infrastructure Pipeline**

Every Refit client gets this request/response pipeline automatically:

```
HTTP Request → Authentication → Tracking → Telemetry → Resilience → Refit → Kontent.ai API
                    ↓              ↓          ↓           ↓         ↓
              Bearer Token    SDK Headers  Monitoring   Retry    JSON Mapping
```

### **ClientOptions Base Class**

All SDK options inherit from this foundation:

```csharp
public abstract record ClientOptions
{
    /// <summary>
    /// Your Kontent.ai environment identifier (required).
    /// </summary>
    public required string EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets whether to enable resilience handling (default: true).
    /// When false, no retry, circuit breaker, or timeout policies are applied.
    /// </summary>
    public bool EnableResilience { get; set; } = true;
}

/// <summary>
/// Extension methods for client options that provide base URL and API key resolution.
/// SDKs should implement these extension methods for their specific options types.
/// </summary>
public static class ClientOptionsExtensions
{
    /// <summary>
    /// Gets the base URL for the specified client options.
    /// SDKs must implement their own GetBaseUrl extension method.
    /// </summary>
    public static string GetBaseUrl(this ClientOptions options, object? requestContext = null)
    {
        throw new NotImplementedException($"GetBaseUrl extension method not implemented for {options.GetType().Name}");
    }

    /// <summary>
    /// Gets the API key for the specified client options.
    /// SDKs must implement their own GetApiKey extension method.
    /// </summary>
    public static string? GetApiKey(this ClientOptions options, object? requestContext = null)
    {
        throw new NotImplementedException($"GetApiKey extension method not implemented for {options.GetType().Name}");
    }

    /// <summary>
    /// Validates the client options configuration.
    /// SDKs can call this base validation and add their own custom validation.
    /// </summary>
    public static void Validate(this ClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EnvironmentId))
            throw new InvalidOperationException("EnvironmentId is required");
    }
}
```

### **Extension Method Pattern for SDK Implementation**

The core package uses an **extension method pattern** for `GetBaseUrl()`, `GetApiKey()`, and `Validate()` methods instead of abstract methods. This provides several benefits:

#### **✅ Benefits of Extension Methods**
- **Flexibility**: SDKs can implement different method signatures if needed
- **Testability**: Extension methods are easier to mock and test
- **Immutable Records**: Works perfectly with `record` types that are immutable
- **Optional Implementation**: SDKs only implement the methods they actually need

#### **🔧 How to Implement Extension Methods**

```csharp
// 1. Define your options as a record
public record MyClientOptions : ClientOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.example.com";
}

// 2. Create extension methods for your specific options type
public static class MyClientOptionsExtensions  
{
    public static string GetBaseUrl(this MyClientOptions options, object? requestContext = null)
    {
        return options.BaseUrl;
    }

    public static string? GetApiKey(this MyClientOptions options, object? requestContext = null)
    {
        return options.ApiKey;
    }

    public static void Validate(this MyClientOptions options)
    {
        // Call base validation first
        ((ClientOptions)options).Validate();
        
        // Add your custom validation
        if (string.IsNullOrEmpty(options.ApiKey))
            throw new ArgumentException("API key is required");
    }
}
```

## 🛠️ **Usage Examples for SDK Consumers**

### **Simple Registration**

```csharp
// Program.cs - Minimal setup
builder.Services.AddKontentDelivery(options =>
{
    options.EnvironmentId = "your-environment-id";
    options.UsePreviewApi = false;
    // BaseUrl is automatically determined by GetBaseUrl() implementation
});

// Usage in controllers/services
public class ProductController(DeliveryClient deliveryClient)
{
    public async Task<IActionResult> GetProducts()
    {
        var products = await deliveryClient.GetItemsAsync<Product>();
        return Ok(products);
    }
}
```

### **Disabling Resilience**

```csharp
// For scenarios where you want to handle resilience yourself or in testing
builder.Services.AddKontentDelivery(options =>
{
    options.EnvironmentId = "your-environment-id";
    options.EnableResilience = false; // Disables retry, circuit breaker, and timeout policies
});
```

### **Configuration-based Setup**

```csharp
// appsettings.json
{
  "Kontent": {
    "Delivery": {
      "EnvironmentId": "your-env-id",
      "UsePreviewApi": false,
      "PreviewApiKey": "your-preview-key"
    },
    "Management": {
      "EnvironmentId": "your-env-id", 
      "ManagementApiKey": "your-management-key"
    }
  }
}

// Program.cs
builder.Services.AddKontentDelivery(builder.Configuration.GetSection("Kontent:Delivery").Bind);
builder.Services.AddKontentManagement(builder.Configuration.GetSection("Kontent:Management").Bind);
```

### **Advanced Customization**

```csharp
// Custom telemetry, resilience, and Refit settings
builder.Services.AddCore(coreOptions =>
{
    coreOptions.ApiUsageListener = new CustomTelemetryListener();
    coreOptions.SdkIdentity = new SdkIdentity("MyApp.Integration", new Version(2, 1, 0));
});

builder.Services.AddKontentDelivery(
    options =>
    {
        options.EnvironmentId = "production-env";
        options.BaseUrl = "https://deliver.kontent.ai";
    },
    configureRefitSettings: settings =>
    {
        // Customize JSON serialization
        settings.ContentSerializer = new CustomJsonContentSerializer();
    },
    configureResilience: (builder, clientOptions) =>
    {
        // Custom retry strategy for high-traffic scenarios
        builder.AddRetry(r =>
        {
            r.MaxRetryAttempts = 5;
            r.BaseDelay = TimeSpan.FromMilliseconds(100);
        });
    });
```

## 🔧 **Refit Configuration & JSON Handling**

### **Default Refit Settings**

The core package provides optimized defaults for Kontent.ai APIs:

```csharp
// Automatically configured by CoreOptions.CreateDefaultRefitSettings()
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};

var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
};
```

### **Authentication Handling**

Authentication is **completely automatic** - just configure your SDK-specific API keys:

```csharp
// For Delivery API (Preview)
services.AddKontentDelivery(options =>
{
    options.UsePreviewApi = true;
    options.PreviewApiKey = "your-preview-api-key"; // Automatically becomes "Bearer your-preview-api-key"
});

// For Management API
services.AddKontentManagement(options =>
{
    options.ManagementApiKey = "your-management-api-key"; // Automatically becomes "Bearer your-management-api-key"
});
```

## 📊 **Monitoring & Telemetry**

### **Built-in API Usage Tracking**

Every request is automatically monitored through `IApiUsageListener`:

```csharp
// Implement custom telemetry
public class ApplicationInsightsTelemetry : IApiUsageListener
{
    public async Task OnRequestStartAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log request start, add correlation IDs, etc.
    }

    public async Task OnRequestEndAsync(
        HttpRequestMessage request, 
        HttpResponseMessage? response, 
        Exception? exception, 
        TimeSpan elapsed, 
        CancellationToken cancellationToken)
    {
        // Log performance metrics, errors, response times
        // Track API usage patterns, rate limiting, etc.
    }
}

// Register your telemetry
services.AddCore(options =>
{
    options.ApiUsageListener = new ApplicationInsightsTelemetry();
});
```

### **Automatic Headers**

Every request gets proper tracking headers automatically:

```
X-KC-SDKID: nuget.org;Kontent.Ai.Delivery;2.0.0
X-KC-SOURCE: nuget.org;MyApp;1.5.2
```

## 🛡️ **Resilience & Error Handling**

### **Production-Ready Defaults**

All HTTP clients get automatic resilience policies by default (when `EnableResilience = true`):

- **Retry Policy**: Automatic retry with exponential backoff
- **Circuit Breaker**: Prevents cascading failures
- **Timeout**: Per-request timeout handling

**Note**: Resilience can be completely disabled by setting `EnableResilience = false` in your client options, which removes all retry, circuit breaker, and timeout policies.

### **Customizable Per SDK**

```csharp
// Different resilience for read vs write operations
services.AddKontentDelivery(options => { /* ... */ }); // Uses defaults (good for reads)

services.AddKontentManagement(
    options => { /* ... */ },
    configureResilience: (builder, clientOptions) =>
    {
        // Conservative settings for write operations - you have access to the pipeline builder
        // Note: Default strategies (retry, timeout, circuit breaker) are already added
        // You can add additional strategies or customize existing ones
    });
```

## 🔄 **Migration Benefits from Current SDKs**

### **Before: Manual HTTP Client Management**
```csharp
// Old approach - lots of boilerplate
public class DeliveryClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<T> GetItemAsync<T>(string codename)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/{environmentId}/items/{codename}");
        
        // Manual authentication
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
        // Manual headers
        request.Headers.Add("X-KC-SDKID", sdkTrackingValue);
        
        // Manual error handling
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        // Manual JSON deserialization
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, jsonOptions);
    }
}
```

### **After: Refit-Based Declarative APIs**
```csharp
// New approach - clean and declarative
public interface IDeliveryApi
{
    [Get("/{environmentId}/items/{codename}")]
    Task<ItemResponse<T>> GetItemAsync<T>(string environmentId, string codename);
}

public class DeliveryClient
{
    private readonly IDeliveryApi _api;
    
    public async Task<T> GetItemAsync<T>(string codename)
    {
        var response = await _api.GetItemAsync<T>(_options.EnvironmentId, codename);
        return response.Item;
        // Authentication, headers, JSON, error handling, resilience = all automatic!
    }
}
```

### **Key Improvements**

1. **90% Less Boilerplate**: Refit handles HTTP mechanics automatically
2. **Type Safety**: Compile-time validation of API contracts
3. **Testability**: Easy to mock Refit interfaces for unit tests
4. **Maintainability**: API changes are centralized in interfaces
5. **Performance**: Optimized JSON settings and HTTP client reuse
6. **Reliability**: Built-in resilience and monitoring

## 🎯 **Review Goals: What We're Proposing**

This architecture demonstrates our vision for **Kontent.ai .NET SDK v2**:

### **✅ For SDK Authors**
- **Faster Development**: Focus on business logic, not HTTP plumbing
- **Consistency**: All SDKs use the same infrastructure and patterns  
- **Reliability**: Production-ready resilience and monitoring out of the box
- **Maintainability**: Declarative APIs are easier to evolve and test

### **✅ For SDK Users**
- **Simpler Setup**: Minimal configuration with smart defaults
- **Better Performance**: Optimized HTTP clients and JSON serialization
- **Built-in Monitoring**: Telemetry and tracking without extra work
- **Production Ready**: Resilience policies tuned for Kontent.ai APIs

### **✅ For the Platform**
- **Unified Telemetry**: Consistent tracking across all .NET SDKs
- **Better Support**: Detailed request/response logging and error context
- **Performance Insights**: Built-in metrics for API usage patterns

**The goal is to move from "HTTP client library" to "declarative API SDK framework" that handles all the infrastructure concerns automatically while providing a clean, type-safe developer experience.**

---

## 📚 **Complete Integration Examples**

> [!NOTE]
> These are **example implementations** showing how delivery and management SDKs would integrate with this core package. The actual implementation details may differ.

<details>
<summary><strong>📦 Example: Complete Delivery SDK Implementation</strong></summary>

```csharp
// 1. Refit API Interface
public interface IDeliveryApi
{
    [Get("/{environmentId}/items")]
    Task<ItemsResponse<TItem>> GetItemsAsync<TItem>(
        string environmentId,
        [Query] string? elements = null,
        [Query] string? filter = null,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);

    [Get("/{environmentId}/items/{codename}")]
    Task<ItemResponse<TItem>> GetItemAsync<TItem>(
        string environmentId,
        string codename,
        [Query] string? elements = null,
        CancellationToken cancellationToken = default);

    [Get("/{environmentId}/types")]
    Task<TypesResponse> GetTypesAsync(
        string environmentId,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);
}

// 2. Options Configuration
public record DeliveryClientOptions : ClientOptions
{
    public bool UsePreviewApi { get; set; } = false;
    public string? PreviewApiKey { get; set; }
    public bool IncludeTotalCount { get; set; } = false;
}

// Extension methods for DeliveryClientOptions
public static class DeliveryClientOptionsExtensions
{
    public static void Validate(this DeliveryClientOptions options)
    {
        ((ClientOptions)options).Validate();
        if (options.UsePreviewApi && string.IsNullOrEmpty(options.PreviewApiKey))
        {
            throw new ArgumentException("Preview API key is required when UsePreviewApi is true.");
        }
    }
}

// 3. Client Implementation
public class DeliveryClient
{
    private readonly IDeliveryApi _api;
    private readonly DeliveryClientOptions _options;

    public DeliveryClient(IDeliveryApi api, IOptions<DeliveryClientOptions> options)
    {
        _api = api;
        _options = options.Value;
    }

    public async Task<IEnumerable<TItem>> GetItemsAsync<TItem>(
        string? filter = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.GetItemsAsync<TItem>(
            _options.EnvironmentId,
            filter: filter,
            limit: limit,
            cancellationToken: cancellationToken);

        return response.Items;
    }

    public async Task<TItem> GetItemAsync<TItem>(
        string codename,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.GetItemAsync<TItem>(
            _options.EnvironmentId,
            codename,
            cancellationToken: cancellationToken);

        return response.Item;
    }
}

// 4. Registration Extensions
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKontentDelivery(
        this IServiceCollection services,
        Action<DeliveryClientOptions> configureOptions)
    {
        services.AddClient<IDeliveryApi, DeliveryClientOptions>(configureOptions);
        services.AddTransient<DeliveryClient>();
        return services;
    }
}

// 5. Usage
// Program.cs
builder.Services.AddKontentDelivery(options =>
{
    options.EnvironmentId = "your-env-id";
    options.UsePreviewApi = true;
    options.PreviewApiKey = "your-preview-key";
});

// Controller
public class ProductController(DeliveryClient deliveryClient)
{
    public async Task<IActionResult> GetProducts()
    {
        var products = await deliveryClient.GetItemsAsync<Product>(limit: 10);
        return Ok(products);
    }
}
```

</details>

<details>
<summary><strong>⚙️ Example: Complete Management SDK Implementation</strong></summary>

```csharp
// 1. Refit API Interface
public interface IManagementApi
{
    [Get("/v2/projects/{environmentId}/items")]
    Task<ItemsResponse> GetItemsAsync(
        string environmentId,
        [Query] int? skip = null,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default);

    [Post("/v2/projects/{environmentId}/items")]
    Task<ItemResponse> CreateItemAsync(
        string environmentId,
        [Body] CreateItemRequest request,
        CancellationToken cancellationToken = default);

    [Put("/v2/projects/{environmentId}/items/{itemId}")]
    Task<ItemResponse> UpdateItemAsync(
        string environmentId,
        string itemId,
        [Body] UpdateItemRequest request,
        CancellationToken cancellationToken = default);

    [Delete("/v2/projects/{environmentId}/items/{itemId}")]
    Task DeleteItemAsync(
        string environmentId,
        string itemId,
        CancellationToken cancellationToken = default);
}

// 2. Options Configuration
public record ManagementClientOptions : ClientOptions
{
    public string? ManagementApiKey { get; set; }
    public bool UseProductionApi { get; set; } = true;
}

// Extension methods for ManagementClientOptions
public static class ManagementClientOptionsExtensions
{
    public static void Validate(this ManagementClientOptions options)
    {
        ((ClientOptions)options).Validate();
        if (string.IsNullOrEmpty(options.ManagementApiKey))
        {
            throw new ArgumentException("Management API key is required.");
        }
    }
}

// 3. Client Implementation
public class ManagementClient
{
    private readonly IManagementApi _api;
    private readonly ManagementClientOptions _options;

    public ManagementClient(IManagementApi api, IOptions<ManagementClientOptions> options)
    {
        _api = api;
        _options = options.Value;
    }

    public async Task<ContentItem> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.CreateItemAsync(
            _options.EnvironmentId,
            request,
            cancellationToken);

        return response.Item;
    }

    public async Task<ContentItem> UpdateItemAsync(
        string itemId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.UpdateItemAsync(
            _options.EnvironmentId,
            itemId,
            request,
            cancellationToken);

        return response.Item;
    }

    public async Task DeleteItemAsync(
        string itemId,
        CancellationToken cancellationToken = default)
    {
        await _api.DeleteItemAsync(_options.EnvironmentId, itemId, cancellationToken);
    }
}

// 4. Registration Extensions
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKontentManagement(
        this IServiceCollection services,
        Action<ManagementClientOptions> configureOptions)
    {
        services.AddClient<IManagementApi, ManagementClientOptions>(
            configureOptions,
            configureResilience: (builder, clientOptions) =>
            {
                // Note: Default strategies (retry, timeout, circuit breaker) are already added
                // For write operations, you might want to add additional conservative strategies
                // or customize behavior based on client options
            });

        services.AddTransient<ManagementClient>();
        return services;
    }
}

// 5. Usage
// Program.cs
builder.Services.AddKontentManagement(options =>
{
    options.EnvironmentId = "your-env-id";
    options.ManagementApiKey = "your-management-key";
});

// Service
public class ContentService(ManagementClient managementClient)
{
    public async Task<ContentItem> CreateArticleAsync(string title, string content)
    {
        var request = new CreateItemRequest
        {
            Name = title,
            Type = "article",
            Elements = new[]
            {
                new ElementRequest { Element = "title", Value = title },
                new ElementRequest { Element = "content", Value = content }
            }
        };

        return await managementClient.CreateItemAsync(request);
    }
}
```

</details>

## 🤝 **Contributing to the Architecture**

This package represents our proposed direction for Kontent.ai .NET SDK v2. When reviewing this architecture:

1. **Evaluate the Refit-centered approach** - Does this provide the right balance of simplicity and power?
2. **Review the automatic infrastructure** - Are the defaults appropriate for most use cases?
3. **Consider the developer experience** - How does this compare to manual HTTP client patterns?
4. **Assess maintainability** - Will this architecture scale across multiple SDK packages?

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
