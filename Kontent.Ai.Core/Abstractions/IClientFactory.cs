namespace Kontent.Ai.Core.Abstractions;

/// <summary>
/// Factory interface for creating Kontent.ai clients with different configurations.
/// Supports both named clients (for DI scenarios) and standalone clients (for direct instantiation).
/// </summary>
public interface IClientFactory<TClient, TOptions> 
    where TOptions : Configuration.ClientOptions
{
    /// <summary>
    /// Creates a named client using the configuration registered for the specified name.
    /// This method is used in DI scenarios where clients are registered with different configurations.
    /// </summary>
    /// <param name="name">The name of the client configuration to use.</param>
    /// <returns>A configured client instance.</returns>
    TClient CreateClient(string name);

    /// <summary>
    /// Creates a client using the default configuration.
    /// </summary>
    /// <returns>A client instance with default configuration.</returns>
    TClient CreateClient();

    /// <summary>
    /// Creates a standalone client with the specified options.
    /// This method is used for direct instantiation scenarios without DI.
    /// </summary>
    /// <param name="options">The client configuration options.</param>
    /// <returns>A configured client instance.</returns>
    TClient CreateClient(TOptions options);

    /// <summary>
    /// Creates a standalone client with the specified options and custom HttpClient.
    /// This method provides maximum flexibility for advanced scenarios.
    /// </summary>
    /// <param name="options">The client configuration options.</param>
    /// <param name="httpClient">A custom HttpClient instance to use.</param>
    /// <returns>A configured client instance.</returns>
    TClient CreateClient(TOptions options, HttpClient httpClient);
} 