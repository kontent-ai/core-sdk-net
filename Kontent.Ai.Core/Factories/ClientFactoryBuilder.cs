using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.Core.Factories;

/// <summary>
/// Simple concrete builder for configuring multiple named clients without inheritance complexity.
/// Uses factory delegates instead of abstract methods, making it easier for SDK authors to use.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client, must inherit from ClientOptions.</typeparam>
public sealed class ClientFactoryBuilder<TClient, TOptions>
    where TOptions : ClientOptions, new()
{
    private readonly IServiceCollection _services;
    private readonly Dictionary<string, ClientConfiguration<TOptions>> _clientConfigurations = [];
    private readonly Func<TOptions, TClient> _clientFactory;

    /// <summary>
    /// Initializes a new instance of the ClientFactoryBuilder.
    /// </summary>
    /// <param name="services">The service collection to register clients with.</param>
    /// <param name="clientFactory">Factory delegate to create client instances from options.</param>
    public ClientFactoryBuilder(IServiceCollection services, Func<TOptions, TClient> clientFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(clientFactory);
        
        _services = services;
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// Adds a named client configuration to the builder.
    /// </summary>
    /// <param name="name">The unique name for this client configuration.</param>
    /// <param name="configureOptions">Action to configure the client options.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient setup.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public ClientFactoryBuilder<TClient, TOptions> AddClient(
        string name,
        Action<TOptions> configureOptions,
        Action<IHttpClientBuilder>? configureClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configureOptions);
        
        if (_clientConfigurations.ContainsKey(name))
            throw new ArgumentException($"A client with name '{name}' is already registered.", nameof(name));

        var options = new TOptions
        {
            HttpClientName = $"kontent-ai-client-{name}"
        };
        configureOptions(options);

        _clientConfigurations[name] = new ClientConfiguration<TOptions>(options, configureClient);

        return this;
    }

    /// <summary>
    /// Builds and registers all configured clients with the service collection.
    /// </summary>
    /// <returns>The service collection for further configuration.</returns>
    public IServiceCollection Build()
    {
        // Register all client configurations
        foreach (var (name, config) in _clientConfigurations)
        {
            _services.Configure<TOptions>(name, opt =>
            {
                opt.EnvironmentId = config.Options.EnvironmentId;
                opt.ApiKey = config.Options.ApiKey;
                opt.BaseUrl = config.Options.BaseUrl;
                opt.HttpClientName = config.Options.HttpClientName;
                opt.MaxRetryAttempts = config.Options.MaxRetryAttempts;
            });

            // Register named HttpClient if custom configuration is provided
            if (config.ConfigureHttpClient != null)
            {
                var httpClientBuilder = _services.AddHttpClient(config.Options.HttpClientName);
                config.ConfigureHttpClient(httpClientBuilder);
            }
        }

        // Register the concrete factory implementation
        _services.AddSingleton<IMultipleClientFactory<TClient, TOptions>>(serviceProvider =>
            new ConcreteMultipleClientFactory<TClient, TOptions>(serviceProvider, _clientFactory, _clientConfigurations.Keys));

        return _services;
    }
}

/// <summary>
/// Internal configuration holder for a client.
/// </summary>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
internal record ClientConfiguration<TOptions>(TOptions Options, Action<IHttpClientBuilder>? ConfigureHttpClient)
    where TOptions : ClientOptions;

/// <summary>
/// Concrete implementation of IMultipleClientFactory that uses factory delegates.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
internal sealed class ConcreteMultipleClientFactory<TClient, TOptions>(
    IServiceProvider serviceProvider,
    Func<TOptions, TClient> clientFactory,
    IEnumerable<string> registeredNames) : IMultipleClientFactory<TClient, TOptions>
    where TOptions : ClientOptions
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Func<TOptions, TClient> _clientFactory = clientFactory;
    private readonly HashSet<string> _registeredNames = [.. registeredNames];

    public TClient CreateClient(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        if (!_registeredNames.Contains(name))
            throw new ArgumentException($"No client with name '{name}' is registered.", nameof(name));

        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();
        var options = optionsMonitor.Get(name);
        
        return _clientFactory(options);
    }

    public IEnumerable<string> GetRegisteredClientNames() => _registeredNames;

    public bool IsClientRegistered(string name) => _registeredNames.Contains(name);
} 