namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Base configuration options for Kontent.ai clients.
/// SDKs should inherit from this class to create their specific options.
/// </summary>
public abstract record ClientOptions
{
    /// <summary>
    /// Gets or sets the Kontent.ai environment identifier.
    /// This property is required and must be provided during initialization.
    /// </summary>
    public required string EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable resilience handling.
    /// </summary>
    public bool EnableResilience { get; set; } = true;
}

/// <summary>
/// Extension methods for client options that provide base URL and API key resolution.
/// SDKs should implement these extension methods for their specific options types.
/// </summary>
public static class ClientOptionsExtensions
{
    /// <summary>
    /// Gets the base URL for the specified client options.
    /// This is a placeholder method - SDKs should implement their own extension methods.
    /// </summary>
    /// <param name="options">The client options instance.</param>
    /// <param name="requestContext">Optional context information about the current request.</param>
    /// <returns>The base URL to use for HTTP requests.</returns>
    /// <exception cref="NotImplementedException">Always thrown - SDKs must implement their own GetBaseUrl extension.</exception>
    public static string GetBaseUrl(this ClientOptions options)
    {
        throw new NotImplementedException($"GetBaseUrl extension method not implemented for {options.GetType().Name}. SDKs must implement their own GetBaseUrl extension method.");
    }

    /// <summary>
    /// Gets the API key for the specified client options.
    /// This is a placeholder method - SDKs should implement their own extension methods.
    /// </summary>
    /// <param name="options">The client options instance.</param>
    /// <param name="requestContext">Optional context information about the current request.</param>
    /// <returns>The API key to use for authentication, or null if no authentication is required.</returns>
    /// <exception cref="NotImplementedException">Always thrown - SDKs must implement their own GetApiKey extension.</exception>
    public static string? GetApiKey(this ClientOptions options)
    {
        throw new NotImplementedException($"GetApiKey extension method not implemented for {options.GetType().Name}. SDKs must implement their own GetApiKey extension method.");
    }

    /// <summary>
    /// Validates the client options.
    /// </summary>
    /// <param name="options">The client options instance.</param>
    /// <exception cref="InvalidOperationException">Thrown when the client options are invalid.</exception>
    public static void Validate(this ClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EnvironmentId))
        {
            throw new InvalidOperationException("EnvironmentId is required");
        }
    }
}
