using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Handlers;
using Kontent.Ai.Core.Modules.ApiUsageListener;
using Refit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.CircuitBreaker;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Kontent.ai core services.
/// </summary>
/// <remarks>
/// The primary method for registering clients is AddClient&lt;T&gt;() which wraps AddRefitClient&lt;T&gt;
/// and automatically configures all necessary services including options, resilience, and handlers.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a generic Refit client with all necessary Kontent.ai infrastructure.
    /// This is the main registration method that wraps AddRefitClient and adds all required services.
    /// </summary>
    /// <typeparam name="T">The Refit interface type to register.</typeparam>
    /// <typeparam name="TOptions">The client options type.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configureClientOptions">Action to configure the client options.</param>
    /// <param name="configureRefitSettings">Optional action to configure Refit settings.</param>
    /// <param name="configureHttpClient">Optional action to configure the HttpClient.</param>
    /// <param name="configureResilience">Optional action to configure resilience strategies. Applied only when EnableResilience is true.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// If <see cref="ClientOptions.EnableResilience"/> is false in the client options, 
    /// no resilience strategies will be added regardless of the <paramref name="configureResilience"/> configuration.
    /// </remarks>
    public static IServiceCollection AddClient<T, TOptions>(
        this IServiceCollection services,
        Action<TOptions> configureClientOptions,
        Action<RefitSettings>? configureRefitSettings = null,
        Action<HttpClient>? configureHttpClient = null,
        Action<ResiliencePipelineBuilder<HttpResponseMessage>, TOptions>? configureResilience = null)
        where T : class
        where TOptions : ClientOptions
    {
        services.AddCore();
        services.Configure(configureClientOptions);
        services.PostConfigure(ValidateOptions<TOptions>());
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TOptions>>().Value);
        services.AddSingleton<AuthenticationHandler<TOptions>>();

        var clientBuilder = services.AddRefitClient<T>(sp =>
            {
                var settings = sp.GetRequiredService<RefitSettings>();
                configureRefitSettings?.Invoke(settings);
                return settings;
            })
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<TOptions>();
                httpClient.BaseAddress = new Uri(options.GetBaseUrl());
                configureHttpClient?.Invoke(httpClient);
            })
            .AddRequestHandlers<TOptions>()
            .AddResilienceHandler("ResilienceHandler", (builder, context) =>
            {
                var options = context.GetOptions<TOptions>();

                if (options.EnableResilience)
                {
                    // Add default strategies first
                    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>());
                    builder.AddTimeout(new TimeoutStrategyOptions());
                    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>());

                    // Allow user to customize or override with additional strategies
                    configureResilience?.Invoke(builder, options);
                }
            });


        return services;
    }

    /// <summary>
    /// Registers a generic Refit client whose TOptions come from the given configuration section.
    /// </summary>
    /// <typeparam name="T">The Refit interface type to register.</typeparam>
    /// <typeparam name="TOptions">The client options type.</typeparam>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configurationSection">The configuration section to bind from.</param>
    public static IServiceCollection AddClient<T, TOptions>(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        Action<RefitSettings>? configureRefit = null,
        Action<HttpClient>? configureHttpClient = null,
        Action<ResiliencePipelineBuilder<HttpResponseMessage>, TOptions>? configureResilience = null)
        where T : class
        where TOptions : ClientOptions
        => services.AddClient<T, TOptions>(
            configurationSection.Bind,
            configureRefit,
            configureHttpClient,
            configureResilience
        );

    /// <summary>
    /// Registers core Kontent.ai services including handlers, telemetry, and JSON settings.
    /// This method is automatically called by AddClient().
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configureCore">Optional action to configure core services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        Action<CoreOptions>? configureCore = null)
    {
        services.AddOptions<CoreOptions>()
                .Configure(opts => configureCore?.Invoke(opts));

        services.TryAddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CoreOptions>>().Value;
            var settings = CoreOptions.CreateDefaultRefitSettings();
            return settings;
        });

        services.TryAddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CoreOptions>>().Value;
            return opts.ApiUsageListener ?? DefaultApiUsageListener.Instance;
        });

        services.TryAddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CoreOptions>>().Value;
            return opts.SdkIdentity ?? SdkIdentity.Core;
        });

        services.TryAddSingleton<TelemetryHandler>();
        services.TryAddSingleton<TrackingHandler>();

        return services;
    }

    /// <summary>
    /// Creates a validation action for client options that wraps validation exceptions with more context.
    /// </summary>
    /// <typeparam name="TOptions">The client options type to validate.</typeparam>
    /// <returns>An action that validates the options and provides better error messages.</returns>
    private static Action<TOptions> ValidateOptions<TOptions>() where TOptions : ClientOptions
    {
        return options =>
        {
            try
            {
                options.Validate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Client options validation failed: {ex.Message}", ex);
            }
        };
    }
}
