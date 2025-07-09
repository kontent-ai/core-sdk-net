namespace Kontent.Ai.Core.Abstractions;

/// <summary>
/// Factory interface for creating multiple named Kontent.ai clients with different configurations.
/// This interface supports the factory pattern where consumers can create multiple clients with different configurations.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
public interface IMultipleClientFactory<TClient, TOptions> 
    where TOptions : Configuration.ClientOptions
{
    /// <summary>
    /// Creates a named client using the configuration registered for the specified name.
    /// This method is used to retrieve clients that were configured during service registration.
    /// </summary>
    /// <param name="name">The name of the client configuration to use.</param>
    /// <returns>A configured client instance.</returns>
    /// <exception cref="ArgumentException">Thrown when no client with the specified name is registered.</exception>
    TClient CreateClient(string name);

    /// <summary>
    /// Gets the names of all registered client configurations.
    /// </summary>
    /// <returns>A collection of registered client names.</returns>
    IEnumerable<string> GetRegisteredClientNames();

    /// <summary>
    /// Checks if a client with the specified name is registered.
    /// </summary>
    /// <param name="name">The client name to check.</param>
    /// <returns>True if a client with the specified name is registered; otherwise, false.</returns>
    bool IsClientRegistered(string name);
} 