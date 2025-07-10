using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Factories;
using Kontent.Ai.Core.Handlers;
using Kontent.Ai.Core.Modules.ApiUsageListener;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
    /// <typeparam name="TOptions">The options type for the client, must inherit from NamedClientOptions.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="clientFactory">Factory delegate to create client instances from options and HttpClient.</param>
    /// <param name="configureFactory">Action to configure the multiple client factory using the builder.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMultipleClientFactory<TClient, TOptions>(
        this IServiceCollection services,
        Func<TOptions, HttpClient, TClient> clientFactory,
        Action<ClientFactoryBuilder<TClient, TOptions>> configureFactory)
        where TOptions : NamedClientOptions, new()
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
    /// Registers a single client instance for simple scenarios.
    /// This is the preferred method for most users who only need one client instance.
    /// HttpClient name is generated automatically.
    /// </summary>
    public static IServiceCollection AddClient<TClient, TOptions>(
        this IServiceCollection services,
        TOptions options,
        Func<TOptions, HttpClient, TClient> clientFactory,
        Action<IHttpClientBuilder>? configureHttpClient = null)
        where TClient : class
        where TOptions : ClientOptions
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clientFactory);

        // Generate a default HttpClient name for simple scenarios
        var httpClientName = $"kontent-ai-{typeof(TClient).Name.ToLowerInvariant()}";

        // Register options as singleton
        services.AddSingleton(options);

        // Register named HttpClient with resilience policies
        var httpClientBuilder = services.AddHttpClient(httpClientName);

        // Add default resilience if enabled
        if (options.EnableDefaultResilience)
        {
            httpClientBuilder.AddDefaultResilienceHandler();
        }

        httpClientBuilder.AddRequestHandlers();

        configureHttpClient?.Invoke(httpClientBuilder);

        // Register the client as singleton
        services.AddSingleton<TClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(httpClientName);

            return clientFactory(options, httpClient);
        });

        return services;
    }

    /// <summary>
    /// Registers a single client with configuration delegate for simple scenarios.
    /// HttpClient name is generated automatically.
    /// </summary>
    public static IServiceCollection AddClient<TClient, TOptions>(
        this IServiceCollection services,
        Action<TOptions> configureOptions,
        Func<TOptions, HttpClient, TClient> clientFactory,
        Action<IHttpClientBuilder>? configureHttpClient = null)
        where TClient : class
        where TOptions : ClientOptions, new()
    {
        var options = new TOptions();
        configureOptions(options);

        return services.AddClient(options, clientFactory, configureHttpClient);
    }

    /// <summary>
    /// Registers a single client with IConfiguration binding for simple scenarios.
    /// Automatically binds configuration section to options and validates them.
    /// HttpClient name is generated automatically.
    /// </summary>
    /// <typeparam name="TClient">The client type to create.</typeparam>
    /// <typeparam name="TOptions">The options type for the client, must inherit from ClientOptions.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The configuration instance to bind options from.</param>
    /// <param name="sectionName">The configuration section name to bind options from.</param>
    /// <param name="clientFactory">Factory delegate to create client instances from options and HttpClient.</param>
    /// <param name="configureHttpClient">Optional action to configure the HttpClient.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddClient<TClient, TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        Func<TOptions, HttpClient, TClient> clientFactory,
        Action<IHttpClientBuilder>? configureHttpClient = null)
        where TClient : class
        where TOptions : ClientOptions, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(clientFactory);

        var options = new TOptions();
        configuration.GetSection(sectionName).Bind(options);

        return services.AddClient(options, clientFactory, configureHttpClient);
    }
}