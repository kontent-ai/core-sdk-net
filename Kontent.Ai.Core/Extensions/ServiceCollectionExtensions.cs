using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Factories;
using Kontent.Ai.Core.Handlers;
using Kontent.Ai.Core.Modules.ApiUsageListener;
using Microsoft.Extensions.Logging;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Kontent.ai core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core Kontent.ai services including handlers, factories, and default JSON options.
    /// This method should be called before registering specific SDK services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="options">Optional configuration options for core services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, CoreServicesOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        options ??= new CoreServicesOptions();
        
        // Register JSON serialization options (default or configured)
        if (options.ConfigureJsonOptions != null)
        {
            var jsonOptions = RefitSettingsFactory.DefaultJsonOptions();
            options.ConfigureJsonOptions(jsonOptions);
            services.AddSingleton(jsonOptions);
        }
        else
        {
            services.AddSingleton(RefitSettingsFactory.DefaultJsonOptions());
        }
        
        // Register telemetry listener (default or custom)
        services.AddSingleton<IApiUsageListener>(options.ApiUsageListener ?? DefaultApiUsageListener.Instance);
        
        // Register SDK identity for tracking (default or custom)
        services.AddSingleton(options.SdkIdentity ?? SdkIdentity.Core);
        
        // Register handlers as transient - they'll be used per HTTP request
        services.AddTransient<TrackingHandler>();
        services.AddTransient<AuthenticationHandler>();
        services.AddTransient<TelemetryHandler>(serviceProvider =>
        {
            var listener = serviceProvider.GetRequiredService<IApiUsageListener>();
            var logger = serviceProvider.GetRequiredService<ILogger<TelemetryHandler>>();
            return new TelemetryHandler(listener, logger, options.TelemetryExceptionBehavior);
        });
        
        return services;
    }

    /// <summary>
    /// Registers a multiple client factory that allows creating multiple named instances of a Kontent.ai client.
    /// This simplified method uses factory delegates instead of complex inheritance patterns.
    /// </summary>
    /// <typeparam name="TClient">The client type to create.</typeparam>
    /// <typeparam name="TOptions">The options type for the client.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="clientFactory">Factory delegate to create client instances from options.</param>
    /// <param name="configureFactory">Action to configure the multiple client factory using the builder.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMultipleClientFactory<TClient, TOptions>(
        this IServiceCollection services,
        Func<TOptions, TClient> clientFactory,
        Action<ClientFactoryBuilder<TClient, TOptions>> configureFactory)
        where TOptions : ClientOptions, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(clientFactory);
        ArgumentNullException.ThrowIfNull(configureFactory);

        // Create the simplified builder and configure it
        var builder = new ClientFactoryBuilder<TClient, TOptions>(services, clientFactory);
        configureFactory(builder);

        // Build the configurations and register the factory
        builder.Build();

        return services;
    }

    /// <summary>
    /// Registers a single client instance with default configuration (no naming required).
    /// This is the preferred method for most users who only need one client instance.
    /// </summary>
    public static IServiceCollection AddClient<TClient, TOptions>(
        this IServiceCollection services,
        TOptions options,
        Func<TOptions, TClient> clientFactory,
        Action<IHttpClientBuilder>? configureHttpClient = null)
        where TClient : class
        where TOptions : ClientOptions
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clientFactory);

        // Register options as singleton
        services.AddSingleton(options);

        // Register named HttpClient (still needs a name internally)
        var httpClientBuilder = services.AddHttpClient(options.HttpClientName);
        configureHttpClient?.Invoke(httpClientBuilder);

        // Register the client as singleton
        services.AddSingleton<TClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(options.HttpClientName);
            
            // You'd need to adapt this based on your client constructor pattern
            return clientFactory(options);
        });

        return services;
    }

    /// <summary>
    /// Registers a single client with configuration delegate.
    /// </summary>
    public static IServiceCollection AddClient<TClient, TOptions>(
        this IServiceCollection services,
        Action<TOptions> configureOptions,
        Func<TOptions, TClient> clientFactory,
        Action<IHttpClientBuilder>? configureHttpClient = null)
        where TClient : class
        where TOptions : ClientOptions, new()
    {
        var options = new TOptions();
        configureOptions(options);
        
        return services.AddClient(options, clientFactory, configureHttpClient);
    }
} 