using Kontent.Ai.Core.Abstractions;

namespace Kontent.Ai.Core.Modules.ApiUsageListener;

/// <summary>
/// Default no-op implementation of IApiUsageListener that performs no operations.
/// This is used as the default implementation when no custom telemetry is configured.
/// </summary>
public sealed class DefaultApiUsageListener : IApiUsageListener
{
    /// <summary>
    /// Gets the singleton instance of the NoOpApiUsageListener.
    /// </summary>
    public static readonly DefaultApiUsageListener Instance = new();

    private DefaultApiUsageListener() { }

    /// <summary>
    /// No-op implementation of OnRequestStartAsync.
    /// </summary>
    /// <param name="request">The HTTP request message being sent.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A completed task.</returns>
    public Task OnRequestStartAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// No-op implementation of OnRequestEndAsync.
    /// </summary>
    /// <param name="request">The HTTP request message that was sent.</param>
    /// <param name="response">The HTTP response message, or null if the request failed before receiving a response.</param>
    /// <param name="exception">The exception that occurred, or null if the request completed successfully.</param>
    /// <param name="elapsed">The time elapsed during the request.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A completed task.</returns>
    public Task OnRequestEndAsync(
        HttpRequestMessage request,
        HttpResponseMessage? response,
        Exception? exception,
        TimeSpan elapsed,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}