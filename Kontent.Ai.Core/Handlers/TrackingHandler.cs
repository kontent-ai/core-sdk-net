using Kontent.Ai.Core.Extensions;

namespace Kontent.Ai.Core.Handlers;

/// <summary>
/// DelegatingHandler that injects SDK and source tracking headers on every request.
/// This handler automatically adds X-KC-SDKID and X-KC-SOURCE headers according to Kontent.ai guidelines.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TrackingHandler.
/// </remarks>
/// <param name="sdkAssembly">Optional SDK assembly to use for tracking. If not provided, attempts to detect automatically.</param>
public sealed class TrackingHandler(Assembly? sdkAssembly = null) : DelegatingHandler
{
    private readonly Assembly? _sdkAssembly = sdkAssembly;

    /// <summary>
    /// Sends an HTTP request with tracking headers added.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Add tracking headers to the request
        request.Headers.AddSdkTrackingHeader(_sdkAssembly);
        request.Headers.AddSourceTrackingHeader();

        // Continue with the request pipeline
        return await base.SendAsync(request, cancellationToken);
    }
} 