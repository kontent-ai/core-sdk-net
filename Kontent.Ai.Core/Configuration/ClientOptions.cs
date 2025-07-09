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
    public string HttpClientName { get; set; } = "kontentai-http-client";

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// This value can be used by consumers to configure retry policies as needed.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the request timeout for individual HTTP operations.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
} 