namespace Kontent.Ai.Core.Tests.TestHelpers;

/// <summary>
/// Test implementation of ClientOptions for unit testing
/// </summary>
public class TestClientOptions : ClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for testing purposes.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for testing purposes.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Returns the configured base URL.
    /// </summary>
    /// <param name="requestContext">Not used in test implementation.</param>
    /// <returns>The configured base URL.</returns>
    public override string GetBaseUrl(object? requestContext = null) => BaseUrl;

    /// <summary>
    /// Returns the configured API key.
    /// </summary>
    /// <param name="requestContext">Not used in test implementation.</param>
    /// <returns>The configured API key.</returns>
    public override string? GetApiKey(object? requestContext = null) => ApiKey;
}