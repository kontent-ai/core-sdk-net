using Kontent.Ai.Core.Abstractions;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Kontent.Ai.Core.Handlers;

/// <summary>
/// DelegatingHandler that hooks into HTTP requests to provide telemetry data through IApiUsageListener.
/// This handler calls OnRequestStart before sending the request and OnRequestEnd after receiving the response.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TelemetryHandler.
/// </remarks>
/// <param name="apiUsageListener">The API usage listener to notify of request events.</param>
public sealed class TelemetryHandler(
    IApiUsageListener listener,
    ILogger<TelemetryHandler> logger) : DelegatingHandler
{
    private readonly IApiUsageListener _listener = listener ?? throw new ArgumentNullException(nameof(listener));
    private readonly ILogger<TelemetryHandler> _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage? response = null;
        Exception? exception = null;

        try
        {
            try
            {
                await _listener.OnRequestStartAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ApiUsageListener.OnRequestStartAsync failed");
            }

            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            try
            {
                await _listener.OnRequestEndAsync(
                    request, response, exception, stopwatch.Elapsed, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ApiUsageListener.OnRequestEndAsync failed");
            }
        }
    }
}
