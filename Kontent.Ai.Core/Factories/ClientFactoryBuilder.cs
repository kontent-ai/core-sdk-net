using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Extensions;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.Core.Factories;

/// <summary>
/// Simple concrete builder for configuring multiple named clients.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client, must inherit from NamedClientOptions.</typeparam>
public sealed class ClientFactoryBuilder<TClient, TOptions>
    where TOptions : NamedClientOptions, new()
{
    private readonly IServiceCollection _services;
    private readonly Dictionary<string, ClientConfiguration<TOptions>> _clientConfigurations = [];
    private readonly Func<TOptions, HttpClient, TClient> _clientFactory;

    /// <summary>
    /// Initializes a new instance of the ClientFactoryBuilder.
    /// </summary>
    /// <param name="services">The service collection to register clients with.</param>
    /// <param name="clientFactory">Factory delegate to create client instances from options and HttpClient.</param>
    public ClientFactoryBuilder(IServiceCollection services, Func<TOptions, HttpClient, TClient> clientFactory)
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
    /// <param name="configureResilience">Optional action to configure custom resilience policies when default resilience is disabled.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public ClientFactoryBuilder<TClient, TOptions> AddClient(
        string name,
        Action<TOptions> configureOptions,
        Action<IHttpClientBuilder>? configureClient = null,
        Action<IHttpClientBuilder>? configureResilience = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configureOptions);

        if (_clientConfigurations.ContainsKey(name))
            throw new ArgumentException($"A client with name '{name}' is already registered.", nameof(name));

        var options = new TOptions
        {
            HttpClientName = SanitizeHttpClientName($"kontent-ai-client-{name}")
        };
        configureOptions(options);

        _clientConfigurations[name] = new ClientConfiguration<TOptions>(options, configureClient, configureResilience);

        return this;
    }

    /// <summary>
    /// Sanitizes the HttpClient name to ensure it doesn't contain invalid characters.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <returns>A sanitized name safe for use as HttpClient name.</returns>
    private static string SanitizeHttpClientName(string name)
    {
        // Replace spaces and other potentially problematic characters with hyphens
        return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-.]", "-");
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
            });

            // Register named HttpClient with default resilience policies and handlers
            var httpClientBuilder = _services.AddHttpClient(config.Options.HttpClientName);

            // Add resilience configuration based on options
            if (config.Options.EnableDefaultResilience)
            {
                httpClientBuilder.AddDefaultResilienceHandler();
            }
            else
            {
                config.ConfigureResilience?.Invoke(httpClientBuilder);
            }

            httpClientBuilder.AddRequestHandlers();

            // Apply any custom configuration provided by the user
            config.ConfigureHttpClient?.Invoke(httpClientBuilder);
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
internal record ClientConfiguration<TOptions>(TOptions Options, Action<IHttpClientBuilder>? ConfigureHttpClient, Action<IHttpClientBuilder>? ConfigureResilience)
    where TOptions : NamedClientOptions;

/// <summary>
/// Concrete implementation of IMultipleClientFactory that uses factory delegates.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
internal sealed class ConcreteMultipleClientFactory<TClient, TOptions>(
    IServiceProvider serviceProvider,
    Func<TOptions, HttpClient, TClient> clientFactory,
    IEnumerable<string> registeredNames) : IMultipleClientFactory<TClient, TOptions>
    where TOptions : NamedClientOptions
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Func<TOptions, HttpClient, TClient> _clientFactory = clientFactory;
    private readonly HashSet<string> _registeredNames = [.. registeredNames];

    public TClient CreateClient(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_registeredNames.Contains(name))
            throw new ArgumentException($"No client with name '{name}' is registered.", nameof(name));

        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();
        var options = optionsMonitor.Get(name);

        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(options.HttpClientName);

        return _clientFactory(options, httpClient);
    }

    public IEnumerable<string> GetRegisteredClientNames() => _registeredNames;

    public bool IsClientRegistered(string name) => _registeredNames.Contains(name);
}