using Kontent.Ai.Core.Abstractions;
using Kontent.Ai.Core.Configuration;
using System.Diagnostics;

namespace Kontent.Ai.Core.Handlers;

/// <summary>
/// DelegatingHandler that hooks into HTTP requests to provide telemetry data through IApiUsageListener.
/// This handler calls OnRequestStart before sending the request and OnRequestEnd after receiving the response.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TelemetryHandler.
/// </remarks>
/// <param name="apiUsageListener">The API usage listener to notify of request events.</param>
public sealed class TelemetryHandler : DelegatingHandler
{
    private readonly IApiUsageListener _listener;
    private readonly ILogger<TelemetryHandler> _logger;
    private readonly TelemetryExceptionBehavior _exceptionBehavior;

    public TelemetryHandler(IApiUsageListener listener, ILogger<TelemetryHandler> logger, IOptions<CoreOptions> coreOptions)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ArgumentNullException.ThrowIfNull(logger);

        _listener = listener;
        _logger = logger;
        _exceptionBehavior = coreOptions.Value.TelemetryExceptionBehavior;
    }

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
                await _listener.OnRequestStartAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleTelemetryException(ex, "OnRequestStartAsync");
            }

            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
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
                    request, response, exception, stopwatch.Elapsed, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleTelemetryException(ex, "OnRequestEndAsync");
            }
        }
    }

    private void HandleTelemetryException(Exception exception, string methodName)
    {
        switch (_exceptionBehavior)
        {
            case TelemetryExceptionBehavior.LogAndContinue:
                _logger.LogWarning(exception, "ApiUsageListener.{MethodName} failed", methodName);
                break;
            case TelemetryExceptionBehavior.ThrowException:
                throw new InvalidOperationException($"Telemetry listener failed in {methodName}. This may indicate a configuration issue.", exception);
            default:
                _logger.LogWarning(exception, "ApiUsageListener.{MethodName} failed", methodName);
                break;
        }
    }
}
