using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Handlers;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for IHttpClientBuilder to configure Kontent.ai clients.
/// </summary>
public static class HttpClientBuilderExtensions
{
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