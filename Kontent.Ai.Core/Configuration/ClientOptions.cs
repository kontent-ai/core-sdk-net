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
    /// Gets or sets the base URL for the Kontent.ai API.
    /// This property is required and must be provided during initialization.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// This is automatically applied to HTTP requests by the AuthenticationHandler with Bearer scheme.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable resilience handling.
    /// </summary>
    public bool EnableResilience { get; set; } = true;

    /// <summary>
    /// Validates the client options.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the client options are invalid.</exception>
    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(EnvironmentId))
        {
            throw new InvalidOperationException("EnvironmentId is required");
        }
    }
}
