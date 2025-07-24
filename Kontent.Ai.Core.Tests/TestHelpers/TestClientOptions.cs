namespace Kontent.Ai.Core.Tests.TestHelpers;

/// <summary>
/// Test implementation of ClientOptions for unit testing
/// </summary>
public record TestClientOptions : ClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for testing purposes.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for testing purposes.
    /// </summary>
    public string? ApiKey { get; set; }
}

/// <summary>
/// Extension methods for TestClientOptions to provide base URL and API key resolution.
/// </summary>
public static class TestClientOptionsExtensions
{
    /// <summary>
    /// Gets the base URL for test client options.
    /// </summary>
    /// <param name="options">The test client options.</param>
    /// <param name="requestContext">Not used in test implementation.</param>
    /// <returns>The configured base URL.</returns>
    public static string GetBaseUrl(this TestClientOptions options, object? requestContext = null) => options.BaseUrl;

    /// <summary>
    /// Gets the API key for test client options.
    /// </summary>
    /// <param name="options">The test client options.</param>
    /// <param name="requestContext">Not used in test implementation.</param>
    /// <returns>The configured API key.</returns>
    public static string? GetApiKey(this TestClientOptions options, object? requestContext = null) => options.ApiKey;
}