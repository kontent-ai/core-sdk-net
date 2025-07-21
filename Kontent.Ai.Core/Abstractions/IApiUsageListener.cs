namespace Kontent.Ai.Core.Abstractions;

/// <summary>
/// Interface for listening to API usage events.
/// Implementations can forward telemetry data to monitoring systems.
/// </summary>
public interface IApiUsageListener
{
    /// <summary>
    /// Called when an HTTP request is about to be sent.
    /// </summary>
    /// <param name="request">The HTTP request message being sent.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnRequestStartAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when an HTTP request has completed (either successfully or with an error).
    /// </summary>
    /// <param name="request">The HTTP request message that was sent.</param>
    /// <param name="response">The HTTP response message, or null if the request failed before receiving a response.</param>
    /// <param name="exception">The exception that occurred, or null if the request completed successfully.</param>
    /// <param name="elapsed">The time elapsed during the request.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnRequestEndAsync(
        HttpRequestMessage request,
        HttpResponseMessage? response,
        Exception? exception,
        TimeSpan elapsed,
        CancellationToken cancellationToken = default);
}