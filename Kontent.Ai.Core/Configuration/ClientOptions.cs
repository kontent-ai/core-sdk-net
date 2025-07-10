namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Base configuration options for Kontent.ai clients.
/// SDK authors should inherit from this class to create their specific options.
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
    /// Different SDKs may have different requirements for this field.
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
    /// Note: This is a configuration value only - retry implementation is the responsibility 
    /// of individual SDKs using policies like Microsoft.Extensions.Http.Resilience or custom handlers.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;
} 