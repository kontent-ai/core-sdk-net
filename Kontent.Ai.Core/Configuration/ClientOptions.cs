namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Base configuration options for Kontent.ai clients.
/// SDK authors should inherit from this class to create their specific options.
/// Uses modern Microsoft.Extensions.Http.Resilience for resilience configuration.
/// </summary>
public abstract class ClientOptions
{
    /// <summary>
    /// Gets or sets the Kontent.ai environment identifier.
    /// </summary>
    public string? EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// Different SDKs may have different requirements for this field.
    /// ⚠️ NOTE: This property is not automatically applied to HttpClient configuration.
    /// SDK authors must manually configure authentication in their registration methods.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Kontent.ai API.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the HTTP client name for named HttpClient instances.
    /// Used by IHttpClientFactory to resolve the correct client configuration.
    /// </summary>
    public string HttpClientName { get; set; } = "kontentai-http-client";

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// This value is used to configure the default retry policy.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the request timeout for individual HTTP operations.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Validates the configuration options.
    /// SDK authors should override this method to add their specific validation logic.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required options are missing or invalid.</exception>
    public virtual void Validate()
    {
        if (string.IsNullOrEmpty(EnvironmentId))
            throw new ArgumentException("Environment ID is required.", nameof(EnvironmentId));

        if (!Guid.TryParse(EnvironmentId, out _))
            throw new ArgumentException($"Invalid environment ID format: {EnvironmentId}", nameof(EnvironmentId));

        if (string.IsNullOrEmpty(BaseUrl))
            throw new ArgumentException("Base URL is required.", nameof(BaseUrl));

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            throw new ArgumentException($"Invalid base URL format: {BaseUrl}", nameof(BaseUrl));
    }
} 