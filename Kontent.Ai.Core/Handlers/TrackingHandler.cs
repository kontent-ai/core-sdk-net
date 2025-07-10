using Kontent.Ai.Core.Configuration;
using Kontent.Ai.Core.Extensions;

namespace Kontent.Ai.Core.Handlers;

/// <summary>
/// DelegatingHandler that injects SDK and source tracking headers on every request.
/// This handler automatically adds X-KC-SDKID and X-KC-SOURCE headers according to Kontent.ai guidelines.
/// </summary>
public sealed class TrackingHandler : DelegatingHandler
{
    private readonly SdkIdentity _sdkIdentity;

    /// <summary>
    /// Initializes a new instance of the TrackingHandler.
    /// </summary>
    /// <param name="sdkIdentity">SDK identity to use for tracking headers.</param>
    public TrackingHandler(SdkIdentity sdkIdentity)
    {
        ArgumentNullException.ThrowIfNull(sdkIdentity);
        _sdkIdentity = sdkIdentity;
    }

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
        request.Headers.AddSdkTrackingHeader(_sdkIdentity);
        request.Headers.AddSourceTrackingHeader();

        // Continue with the request pipeline
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
} 