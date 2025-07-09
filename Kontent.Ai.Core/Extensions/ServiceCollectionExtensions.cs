using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Factories;
using Kontent.Ai.Core.Handlers;
using Kontent.Ai.Core.Modules.ApiUsageListener;
using System.Text.Json;

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
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentCore(this IServiceCollection services)
        => services.AddKontentCore(configureJsonOptions: null, apiUsageListener: null);

    /// <summary>
    /// Registers core Kontent.ai services with custom JSON serialization options.
    /// This method should be called before registering specific SDK services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configureJsonOptions">Action to configure custom JSON serialization options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentCore(this IServiceCollection services, Action<JsonSerializerOptions> configureJsonOptions)
        => services.AddKontentCore(configureJsonOptions, apiUsageListener: null);

    /// <summary>
    /// Registers core Kontent.ai services with a custom API usage listener for telemetry.
    /// This method should be called before registering specific SDK services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="apiUsageListener">The custom API usage listener for telemetry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentCore(this IServiceCollection services, IApiUsageListener apiUsageListener)
        => services.AddKontentCore(configureJsonOptions: null, apiUsageListener);

    /// <summary>
    /// Registers core Kontent.ai services with custom JSON serialization options and a custom API usage listener for telemetry.
    /// This method should be called before registering specific SDK services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configureJsonOptions">Action to configure custom JSON serialization options.</param>
    /// <param name="apiUsageListener">The custom API usage listener for telemetry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKontentCore(
        this IServiceCollection services, 
        Action<JsonSerializerOptions>? configureJsonOptions, 
        IApiUsageListener? apiUsageListener)
    {
        // Register JSON serialization options (default or configured)
        if (configureJsonOptions != null)
        {
            var jsonOptions = RefitSettingsFactory.DefaultJsonOptions();
            configureJsonOptions(jsonOptions);
            services.AddSingleton(jsonOptions);
        }
        else
        {
            services.AddSingleton(RefitSettingsFactory.DefaultJsonOptions());
        }
        
        // Register telemetry listener (default or custom)
        services.AddSingleton<IApiUsageListener>(apiUsageListener ?? DefaultApiUsageListener.Instance);
        
        // Register handlers as transient - they'll be used per HTTP request
        services.AddTransient<TrackingHandler>();
        services.AddTransient<AuthenticationHandler>();
        services.AddTransient<TelemetryHandler>();
        
        return services;
    }

    /// <summary>
    /// Registers a multiple client factory that allows creating multiple named instances of a Kontent.ai client.
    /// This method provides the base infrastructure that SDKs can extend for their specific client types.
    /// </summary>
    /// <typeparam name="TClient">The client type to create.</typeparam>
    /// <typeparam name="TOptions">The options type for the client.</typeparam>
    /// <typeparam name="TFactory">The factory implementation type.</typeparam>
    /// <typeparam name="TBuilder">The builder type for configuring multiple clients.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configureFactory">Action to configure the multiple client factory using the builder.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMultipleClientFactory<TClient, TOptions, TFactory, TBuilder>(
        this IServiceCollection services,
        Action<TBuilder> configureFactory)
        where TOptions : Configuration.ClientOptions
        where TFactory : class, IMultipleClientFactory<TClient, TOptions>
        where TBuilder : MultipleClientFactoryBuilder<TClient, TOptions, TBuilder>
    {
        ArgumentNullException.ThrowIfNull(configureFactory);

        // Create the builder and configure it
        var builder = (TBuilder)Activator.CreateInstance(typeof(TBuilder), services)!;
        configureFactory(builder);

        // Build the configurations and register the factory
        builder.Build();
        services.AddSingleton<IMultipleClientFactory<TClient, TOptions>, TFactory>();

        return services;
    }
} 