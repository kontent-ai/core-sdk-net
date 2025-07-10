using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Handlers;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for IHttpClientBuilder to configure resilience policies for Kontent.ai clients.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds default resilience policies optimized for Kontent.ai APIs.
    /// Includes retry, circuit breaker, and timeout strategies with sensible defaults.
    /// </summary>
    /// <param name="builder">The HttpClientBuilder to configure.</param>
    /// <param name="options">Optional resilience options. If null, uses defaults from ClientOptions.</param>
    /// <returns>The HttpClientBuilder for method chaining.</returns>
    public static IHttpClientBuilder AddDefaultResilienceHandler(
        this IHttpClientBuilder builder,
        ResilienceOptions? options = null)
    {
        options ??= new ResilienceOptions();

        if (!options.EnableDefaultStrategies)
            return builder;

        builder.AddStandardResilienceHandler(configureOptions =>
        {
            // Configure retry strategy
            if (options.Retry.MaxRetryAttempts > 0)
            {
                configureOptions.Retry.MaxRetryAttempts = options.Retry.MaxRetryAttempts;
                configureOptions.Retry.Delay = options.Retry.BaseDelay;
                configureOptions.Retry.MaxDelay = options.Retry.MaxDelay;
                configureOptions.Retry.UseJitter = options.Retry.UseJitter;
                configureOptions.Retry.BackoffType = options.Retry.UseExponentialBackoff 
                    ? DelayBackoffType.Exponential 
                    : DelayBackoffType.Constant;
            }

            // Configure circuit breaker strategy
            configureOptions.CircuitBreaker.FailureRatio = options.CircuitBreaker.FailureRatio;
            configureOptions.CircuitBreaker.SamplingDuration = options.CircuitBreaker.SamplingDuration;
            configureOptions.CircuitBreaker.MinimumThroughput = options.CircuitBreaker.MinimumThroughput;
            configureOptions.CircuitBreaker.BreakDuration = options.CircuitBreaker.BreakDuration;

            // Configure timeout strategy
            configureOptions.TotalRequestTimeout.Timeout = options.Timeout.Timeout;
        });

        return builder;
    }

    /// <summary>
    /// Adds a custom resilience handler with user-defined configuration.
    /// This allows SDK authors to completely customize resilience strategies.
    /// </summary>
    /// <param name="builder">The HttpClientBuilder to configure.</param>
    /// <param name="configureOptions">Action to configure the resilience options.</param>
    /// <returns>The HttpClientBuilder for method chaining.</returns>
    public static IHttpClientBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder,
        Action<HttpStandardResilienceOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);
        builder.AddStandardResilienceHandler(configureOptions);
        return builder;
    }

    /// <summary>
    /// Adds resilience policies configured from ClientOptions.
    /// This is a convenience method that automatically configures resilience based on ClientOptions.
    /// </summary>
    /// <param name="builder">The HttpClientBuilder to configure.</param>
    /// <param name="clientOptions">The client options containing resilience configuration.</param>
    /// <returns>The HttpClientBuilder for method chaining.</returns>
    public static IHttpClientBuilder AddResilienceFromClientOptions(
        this IHttpClientBuilder builder,
        ClientOptions clientOptions)
    {
        ArgumentNullException.ThrowIfNull(clientOptions);
        
        if (!clientOptions.EnableDefaultResilience)
            return builder;

        var resilienceOptions = ResilienceOptions.FromClientOptions(clientOptions);
        return builder.AddDefaultResilienceHandler(resilienceOptions);
    }

    /// <summary>
    /// Adds Kontent.ai handlers in the correct order.
    /// This ensures proper request/response processing with authentication, tracking, and telemetry.
    /// </summary>
    /// <param name="builder">The HttpClientBuilder to configure.</param>
    /// <returns>The HttpClientBuilder for method chaining.</returns>
    public static IHttpClientBuilder AddRequestHandlers(this IHttpClientBuilder builder)
    {
        return builder
            .AddHttpMessageHandler<TelemetryHandler>()
            .AddHttpMessageHandler<TrackingHandler>()
            .AddHttpMessageHandler<AuthenticationHandler>();
    }

    /// <summary>
    /// Adds Kontent.ai handlers for a specific options type.
    /// This allows type-safe authentication handler registration.
    /// </summary>
    /// <typeparam name="TOptions">The client options type.</typeparam>
    /// <param name="builder">The HttpClientBuilder to configure.</param>
    /// <returns>The HttpClientBuilder for method chaining.</returns>
    public static IHttpClientBuilder AddRequestHandlers<TOptions>(this IHttpClientBuilder builder)
        where TOptions : ClientOptions
    {
        return builder
            .AddHttpMessageHandler<TelemetryHandler>()
            .AddHttpMessageHandler<TrackingHandler>()
            .AddHttpMessageHandler<AuthenticationHandler<TOptions>>();
    }
} 