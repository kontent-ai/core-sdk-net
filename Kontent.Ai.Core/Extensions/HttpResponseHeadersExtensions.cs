namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for HttpResponseHeaders.
/// </summary>
public static class HttpResponseHeadersExtensions
{
    private const string ContinuationHeaderName = "X-Continuation";

    /// <summary>
    /// Gets the continuation token from the X-Continuation header for pagination.
    /// </summary>
    /// <param name="headers">The HTTP response headers.</param>
    /// <returns>The continuation token if present; otherwise null.</returns>
    public static string? GetContinuationHeader(this HttpResponseHeaders headers)
        => headers.TryGetValues(ContinuationHeaderName, out var headerValues)
            ? headerValues.FirstOrDefault()
            : null;

    /// <summary>
    /// Tries to get the retry-after value from response headers.
    /// Handles both date-based and delta-based retry-after headers.
    /// </summary>
    /// <param name="headers">The HTTP response headers.</param>
    /// <param name="retryAfter">The retry-after time span if found.</param>
    /// <returns>True if retry-after header exists; otherwise false.</returns>
    public static bool TryGetRetryAfter(this HttpResponseHeaders headers, out TimeSpan retryAfter)
    {
        // Check for date-based retry-after first
        if (headers?.RetryAfter?.Date != null)
        {
            retryAfter = GetPositiveOrZero(headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow);
            return true;
        }

        // Check for delta-based retry-after
        if (headers?.RetryAfter?.Delta != null)
        {
            retryAfter = GetPositiveOrZero(headers.RetryAfter.Delta.GetValueOrDefault(TimeSpan.Zero));
            return true;
        }

        retryAfter = TimeSpan.Zero;
        return false;
    }

    private static TimeSpan GetPositiveOrZero(TimeSpan timeSpan) =>
        timeSpan < TimeSpan.Zero ? TimeSpan.Zero : timeSpan;
}