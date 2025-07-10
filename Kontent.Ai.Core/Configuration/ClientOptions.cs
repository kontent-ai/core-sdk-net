namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Base configuration options for Kontent.ai clients.
/// SDKs should inherit from this class to create their specific options.
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
    /// Gets or sets the HTTP client name for named HttpClient instances.
    /// Used by IHttpClientFactory to resolve the correct client configuration.
    /// </summary>
    public required string HttpClientName { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// This is used by the default resilience policy to configure retry behavior.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to enable the default resilience policy.
    /// When true, the HttpClient will be configured with retry, circuit breaker, and timeout policies.
    /// </summary>
    public bool EnableDefaultResilience { get; set; } = true;

    /// <summary>
    /// Gets or sets the base delay for retry attempts.
    /// Used in exponential backoff calculations for the default retry policy.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the request timeout for individual HTTP requests.
    /// This is separate from the overall HttpClient timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
} 