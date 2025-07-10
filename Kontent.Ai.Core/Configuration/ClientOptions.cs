namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Base configuration options for Kontent.ai clients.
/// SDKs should inherit from this class to create their specific options.
/// Contains only core API configuration - no HttpClient naming concerns.
/// </summary>
public abstract class ClientOptions
{
    /// <summary>
    /// Gets or sets the Kontent.ai environment identifier.
    /// This property is required and must be provided during initialization.
    /// </summary>
    public required string EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// This is automatically applied to HTTP requests by the AuthenticationHandler with Bearer scheme.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Kontent.ai API.
    /// This property is required and must be provided during initialization.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets whether to enable the default resilience policy.
    /// When true, the HttpClient will be configured with sensible default retry, circuit breaker, and timeout policies.
    /// When false, no default resilience is applied, allowing full customization via configureResilience delegates.
    /// </summary>
    public bool EnableDefaultResilience { get; set; } = true;
}

/// <summary>
/// Configuration options for named Kontent.ai clients used in factory patterns.
/// Extends ClientOptions with HttpClient naming for multiple client scenarios.
/// </summary>
public abstract class NamedClientOptions : ClientOptions
{
    /// <summary>
    /// Gets or sets the HTTP client name for named HttpClient instances.
    /// Used by IHttpClientFactory to resolve the correct client configuration.
    /// </summary>
    public required string HttpClientName { get; set; }
}