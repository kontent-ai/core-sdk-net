using Microsoft.Extensions.DependencyInjection;

namespace Kontent.Ai.Core.Factories;

/// <summary>
/// Abstract builder for configuring multiple named clients.
/// SDK authors should inherit from this class to create specific client factory builders.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type for fluent chaining.</typeparam>
/// <remarks>
/// Initializes a new instance of the MultipleClientFactoryBuilder.
/// </remarks>
/// <param name="services">The service collection to register clients with.</param>
public abstract class MultipleClientFactoryBuilder<TClient, TOptions, TBuilder>(IServiceCollection services)
    where TOptions : Configuration.ClientOptions
    where TBuilder : MultipleClientFactoryBuilder<TClient, TOptions, TBuilder>
{
    protected readonly IServiceCollection Services = services ?? throw new ArgumentNullException(nameof(services));
    protected readonly Dictionary<string, TOptions> ClientConfigurations = [];

    /// <summary>
    /// Adds a named client configuration to the builder.
    /// SDK authors should override this method to provide specific client configuration logic.
    /// </summary>
    /// <param name="name">The name of the client configuration.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient setup.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public abstract TBuilder AddClient(
        string name,
        Action<TOptions> configureOptions,
        Action<IHttpClientBuilder>? configureClient = null);

    /// <summary>
    /// Builds and registers all configured clients with the service collection.
    /// SDK authors should override this method to provide specific registration logic.
    /// </summary>
    /// <returns>The service collection for further configuration.</returns>
    public abstract IServiceCollection Build();

    /// <summary>
    /// Helper method to validate that a client name is unique.
    /// </summary>
    /// <param name="name">The client name to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the name is already registered.</exception>
    protected void ValidateUniqueName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name cannot be null or empty.", nameof(name));

        if (ClientConfigurations.ContainsKey(name))
            throw new ArgumentException($"A client with name '{name}' is already registered.", nameof(name));
    }

    /// <summary>
    /// Helper method to create and configure options for a named client.
    /// </summary>
    /// <param name="name">The client name.</param>
    /// <param name="configureOptions">Action to configure the options.</param>
    /// <returns>The configured options instance.</returns>
    protected TOptions CreateAndConfigureOptions(string name, Action<TOptions> configureOptions)
    {
        var options = CreateOptionsInstance();
        options.HttpClientName = $"kontent-ai-{typeof(TClient).Name.ToLowerInvariant()}-{name}";
        configureOptions(options);
        return options;
    }

    /// <summary>
    /// Abstract method that SDK authors must implement to create new instances of their options type.
    /// </summary>
    /// <returns>A new instance of TOptions.</returns>
    protected abstract TOptions CreateOptionsInstance();
} 