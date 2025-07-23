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
    /// Gets the base URL for the current request context.
    /// Each SDK should implement this method to return the appropriate endpoint
    /// based on their specific requirements (e.g., preview vs production, management vs delivery).
    /// </summary>
    /// <param name="requestContext">Optional context information about the current request.</param>
    /// <returns>The base URL to use for HTTP requests.</returns>
    public abstract string GetBaseUrl(object? requestContext = null);

    /// <summary>
    /// Gets the API key for the current request context.
    /// Each SDK should implement this method to return the appropriate API key
    /// based on their specific requirements (e.g., preview key vs secure access key).
    /// </summary>
    /// <param name="requestContext">Optional context information about the current request.</param>
    /// <returns>The API key to use for authentication, or null if no authentication is required.</returns>
    public abstract string? GetApiKey(object? requestContext = null);

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
