using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Extensions;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.Core.Handlers;

/// <summary>
/// DelegatingHandler that reads an API key from ClientOptions and sets Authorization: Bearer header.
/// This handler automatically adds authentication headers to all requests according to the configured client options.
/// </summary>
/// <typeparam name="TOptions">The client options type that inherits from ClientOptions.</typeparam>
public class AuthenticationHandler<TOptions> : DelegatingHandler
    where TOptions : ClientOptions
{
    private readonly IOptionsMonitor<TOptions> _clientOptionsMonitor;
    private readonly string? _optionsName;

    /// <summary>
    /// Initializes a new instance of the AuthenticationHandler with default options.
    /// </summary>
    /// <param name="clientOptionsMonitor">The client options monitor for retrieving authentication configuration.</param>
    public AuthenticationHandler(IOptionsMonitor<TOptions> clientOptionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(clientOptionsMonitor);
        _clientOptionsMonitor = clientOptionsMonitor;
    }

    /// <summary>
    /// Initializes a new instance of the AuthenticationHandler with named options.
    /// </summary>
    /// <param name="clientOptionsMonitor">The client options monitor for retrieving authentication configuration.</param>
    /// <param name="optionsName">The name of the options configuration to use.</param>
    public AuthenticationHandler(IOptionsMonitor<TOptions> clientOptionsMonitor, string optionsName)
    {
        ArgumentNullException.ThrowIfNull(clientOptionsMonitor);
        ArgumentNullException.ThrowIfNull(optionsName);

        _clientOptionsMonitor = clientOptionsMonitor;
        _optionsName = optionsName;
    }

    /// <summary>
    /// Sends an HTTP request with authentication headers added.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get the client options (named or default)
        var clientOptions = string.IsNullOrEmpty(_optionsName)
            ? _clientOptionsMonitor.CurrentValue
            : _clientOptionsMonitor.Get(_optionsName);

        // Add authorization header if API key is configured
        if (!string.IsNullOrWhiteSpace(clientOptions?.ApiKey))
        {
            request.Headers.AddAuthorizationHeader("Bearer", clientOptions.ApiKey);
        }

        // Continue with the request pipeline
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Non-generic AuthenticationHandler that uses the base ClientOptions type.
/// This is provided for convenience when working with the base ClientOptions directly.
/// </summary>
public class AuthenticationHandler : AuthenticationHandler<ClientOptions>
{
    /// <summary>
    /// Initializes a new instance of the AuthenticationHandler with default options.
    /// </summary>
    /// <param name="clientOptionsMonitor">The client options monitor for retrieving authentication configuration.</param>
    public AuthenticationHandler(IOptionsMonitor<ClientOptions> clientOptionsMonitor)
        : base(clientOptionsMonitor)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthenticationHandler with named options.
    /// </summary>
    /// <param name="clientOptionsMonitor">The client options monitor for retrieving authentication configuration.</param>
    /// <param name="optionsName">The name of the options configuration to use.</param>
    public AuthenticationHandler(IOptionsMonitor<ClientOptions> clientOptionsMonitor, string optionsName)
        : base(clientOptionsMonitor, optionsName)
    {
    }
}