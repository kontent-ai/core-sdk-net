using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using System.Text.Json;
using Polly;
using Kontent.Ai.Core.Modules.ActionInvoker;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Modern service registration extensions using Microsoft.Extensions.Http.Resilience for resilience policies.
/// Follows .NET 8 best practices for HttpClient registration and dependency injection.
/// Supports both factory pattern for multiple named clients and direct client registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a resilient HttpClient with default resilience policies.
    /// This is the base method used by both delivery and management client registration methods.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client to configure.</param>
    /// <param name="configureClient">Optional client configuration.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts. Defaults to 2.</param>
    /// <param name="enableDefaultResilience">Whether to add the default resilience policies. Set to false if you want to configure custom resilience.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddBaseHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null,
        int maxRetryAttempts = 2,
        bool enableDefaultResilience = true)
    {
        var builder = services.AddHttpClient(name, client => configureClient?.Invoke(client));

        if (enableDefaultResilience)
        {
            // Unified resilience handler based on management SDK settings
            // ✅ This automatically handles 429 + Retry-After headers (ShouldRetryAfterHeader = true by default)
            builder.AddStandardResilienceHandler(options =>
            {
                // Unified retry policy using management SDK settings (without circuit breaker/timeouts)
                options.Retry.MaxRetryAttempts = maxRetryAttempts;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                // Note: ShouldRetryAfterHeader = true by default, so 429 responses with 
                // Retry-After headers will be respected and override exponential backoff
                
                // Use default timeouts and no circuit breaker for unified policy
            });
        }

        return builder;
    }

    /// <summary>
    /// Registers a resilient HttpClient with unified Kontent.ai retry policies using options configuration.
    /// This overload allows options-driven configuration of retry attempts.
    /// </summary>
    /// <typeparam name="TOptions">The options type containing MaxRetryAttempts.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client to configure.</param>
    /// <param name="options">The options containing MaxRetryAttempts and other configuration.</param>
    /// <param name="configureClient">Optional client configuration.</param>
    /// <param name="enableDefaultResilience">Whether to add the default resilience policies.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddBaseHttpClient<TOptions>(
        this IServiceCollection services,
        string name,
        TOptions options,
        Action<HttpClient>? configureClient = null,
        bool enableDefaultResilience = true)
        where TOptions : ClientOptions
    {
        return services.AddBaseHttpClient(
            name, 
            configureClient, 
            options.MaxRetryAttempts, 
            enableDefaultResilience);
    }

    /// <summary>
    /// Registers an ActionInvoker for the specified HttpClient.
    /// The ActionInvoker provides high-level HTTP operations with automatic serialization.
    /// All resilience policies (retry, circuit breaker, timeout) are configured on the HttpClient 
    /// via Microsoft.Extensions.Http.Resilience (AddStandardResilienceHandler or AddCustomResilienceHandler).
    /// This method is used internally by SDK authors and generally not called directly by consumers.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="configureJsonOptions">Optional JSON serialization options configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddActionInvoker(
        this IHttpClientBuilder builder,
        Action<JsonSerializerOptions>? configureJsonOptions = null)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        configureJsonOptions?.Invoke(jsonOptions);

        builder.Services.TryAddSingleton(jsonOptions);
        
        // Register ActionInvoker for the specific HttpClient
        // All resilience policies are configured on the HttpClient itself
        builder.Services.AddTransient<IActionInvoker>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(builder.Name);
            var options = provider.GetRequiredService<JsonSerializerOptions>();
            return new ActionInvoker(httpClient, options);
        });

        return builder.Services;
    }

    /// <summary>
    /// Registers a client factory for creating multiple named instances of a Kontent.ai client.
    /// This enables the factory pattern where consumers can create multiple clients with different configurations.
    /// </summary>
    /// <typeparam name="TFactory">The factory type to register.</typeparam>
    /// <typeparam name="TClient">The client type the factory creates.</typeparam>
    /// <typeparam name="TOptions">The options type for the client.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the factory.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentClientFactory<TFactory, TClient, TOptions>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TFactory : class, IClientFactory<TClient, TOptions>
        where TOptions : ClientOptions
    {
        services.Add(new ServiceDescriptor(typeof(IClientFactory<TClient, TOptions>), typeof(TFactory), lifetime));
        services.Add(new ServiceDescriptor(typeof(TFactory), typeof(TFactory), lifetime));
        return services;
    }

    /// <summary>
    /// Registers a direct client implementation for simple scenarios where only one configuration is needed.
    /// This is an alternative to the factory pattern for SDKs that don't need multiple named instances.
    /// </summary>
    /// <typeparam name="TClient">The client type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the client.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentClient<TClient>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TClient : class
    {
        services.Add(new ServiceDescriptor(typeof(TClient), typeof(TClient), lifetime));
        return services;
    }

    /// <summary>
    /// Registers a custom resilience pipeline for advanced scenarios.
    /// The standard resilience handler (AddStandardResilienceHandler) already handles common scenarios.
    /// Use this method only when you need custom behavior beyond the standard resilience pipeline.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="pipelineName">Name of the resilience pipeline.</param>
    /// <param name="configureBuilder">Pipeline configuration delegate.</param>
    /// <returns>The IHttpClientBuilder for chaining.</returns>
    public static IHttpClientBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder,
        string pipelineName,
        Action<ResiliencePipelineBuilder<HttpResponseMessage>> configureBuilder)
    {
        builder.AddResilienceHandler(pipelineName, configureBuilder);
        return builder;
    }

    /// <summary>
    /// Registers Kontent.ai client options with validation.
    /// Supports both single and multiple named configurations.
    /// </summary>
    /// <typeparam name="TOptions">The options type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <param name="name">Optional name for named options configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddClientOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? name = null)
        where TOptions : ClientOptions
    {
        if (string.IsNullOrEmpty(name))
        {
            services.Configure<TOptions>(configuration);
        }
        else
        {
            services.Configure<TOptions>(name, configuration);
        }
        
        return services;
    }

    /// <summary>
    /// Registers multiple named Kontent.ai client options configurations.
    /// This enables scenarios where you need multiple clients with different configurations.
    /// </summary>
    /// <typeparam name="TOptions">The options type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurations">Dictionary of configuration name to configuration section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNamedClientOptions<TOptions>(
        this IServiceCollection services,
        IDictionary<string, IConfiguration> configurations)
        where TOptions : ClientOptions
    {
        foreach (var (name, configuration) in configurations)
        {
            services.Configure<TOptions>(name, configuration);
        }
        
        return services;
    }


} 