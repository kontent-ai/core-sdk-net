using Kontent.Ai.Core.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Kontent.Ai.Core.Abstractions;
using ActionInvokerModule = Kontent.Ai.Core.Modules.ActionInvoker;

namespace Kontent.Ai.Core.Modules.ClientFactory;

/// <summary>
/// Base implementation of the Kontent.ai client factory.
/// SDK authors can inherit from this class to create specific client factories.
/// </summary>
/// <typeparam name="TClient">The client type to create.</typeparam>
/// <typeparam name="TOptions">The options type for the client.</typeparam>
public abstract class ClientFactory<TClient, TOptions> : IClientFactory<TClient, TOptions>
    where TOptions : ClientOptions
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly IOptionsMonitor<TOptions>? _optionsMonitor;
    private readonly JsonSerializerOptions? _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the ClientFactory for DI scenarios.
    /// </summary>
    protected ClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<TOptions> optionsMonitor,
        JsonSerializerOptions? jsonOptions = null)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Initializes a new instance of the ClientFactory for standalone scenarios.
    /// </summary>
    protected ClientFactory()
    {
        // Allow parameterless constructor for standalone usage
    }

    /// <summary>
    /// Creates a named client using the configuration registered for the specified name.
    /// </summary>
    public virtual TClient CreateClient(string name)
    {
        if (_httpClientFactory == null || _optionsMonitor == null)
            throw new InvalidOperationException("Factory must be initialized with IHttpClientFactory and IOptionsMonitor for named client creation. Use dependency injection or call CreateClient(options) instead.");

        var httpClient = _httpClientFactory.CreateClient(name);
        var options = _optionsMonitor.Get(name);
        IActionInvoker actionInvoker = new ActionInvokerModule.ActionInvoker(httpClient, _jsonOptions);

        return CreateClientInstance(actionInvoker, options);
    }

    /// <summary>
    /// Creates a client using the default configuration.
    /// </summary>
    public virtual TClient CreateClient()
    {
        if (_httpClientFactory == null || _optionsMonitor == null)
            throw new InvalidOperationException("Factory must be initialized with IHttpClientFactory and IOptionsMonitor for default client creation. Use dependency injection or call CreateClient(options) instead.");

        var httpClient = _httpClientFactory.CreateClient(Options.DefaultName);
        var options = _optionsMonitor.CurrentValue;
        var actionInvoker = new ActionInvokerModule.ActionInvoker(httpClient, _jsonOptions);

        return CreateClientInstance(actionInvoker, options);
    }

    /// <summary>
    /// Creates a standalone client with the specified options.
    /// </summary>
    public virtual TClient CreateClient(TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        var httpClient = CreateStandaloneHttpClient(options);
        var jsonOptions = CreateJsonSerializerOptions(options);
        var actionInvoker = new ActionInvokerModule.ActionInvoker(httpClient, jsonOptions);

        return CreateClientInstance(actionInvoker, options);
    }

    /// <summary>
    /// Creates a standalone client with the specified options and custom HttpClient.
    /// </summary>
    public virtual TClient CreateClient(TOptions options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);

        options.Validate();

        ConfigureStandaloneHttpClient(httpClient, options);
        var jsonOptions = CreateJsonSerializerOptions(options);
        IActionInvoker actionInvoker = new ActionInvokerModule.ActionInvoker(httpClient, jsonOptions);

        return CreateClientInstance(actionInvoker, options);
    }

    /// <summary>
    /// Creates the actual client instance. SDK authors must implement this method.
    /// </summary>
    /// <param name="actionInvoker">The configured action invoker.</param>
    /// <param name="options">The client options.</param>
    /// <returns>A configured client instance.</returns>
    protected abstract TClient CreateClientInstance(IActionInvoker actionInvoker, TOptions options);

    /// <summary>
    /// Creates a standalone HttpClient for non-DI scenarios.
    /// Can be overridden by SDK authors to customize the HttpClient configuration.
    /// </summary>
    protected virtual HttpClient CreateStandaloneHttpClient(TOptions options)
    {
        var httpClient = new HttpClient();
        ConfigureStandaloneHttpClient(httpClient, options);
        return httpClient;
    }

    /// <summary>
    /// Configures the HttpClient for standalone scenarios.
    /// Can be overridden by SDK authors to add custom configuration.
    /// </summary>
    protected virtual void ConfigureStandaloneHttpClient(HttpClient httpClient, TOptions options)
    {
        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            httpClient.BaseAddress = new Uri(options.BaseUrl);
        }

        httpClient.Timeout = options.RequestTimeout;
    }

    /// <summary>
    /// Creates JsonSerializerOptions for the client.
    /// Can be overridden by SDK authors to customize JSON serialization.
    /// </summary>
    protected virtual JsonSerializerOptions CreateJsonSerializerOptions(TOptions options)
    {
        return _jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
} 