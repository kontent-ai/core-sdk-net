namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Configuration options for resilience policies applied to Kontent.ai HTTP clients.
/// Provides sensible defaults for Kontent.ai APIs while allowing customization.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Gets or sets the retry configuration options.
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Gets or sets the circuit breaker configuration options.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout configuration options.
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable the retry strategy.
    /// </summary>
    public bool EnableRetry { get; set; }

    /// <summary>
    /// Gets or sets whether to enable the circuit breaker strategy.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; }

    /// <summary>
    /// Gets or sets whether to enable the timeout strategy.
    /// </summary>
    public bool EnableTimeout { get; set; }

    /// <summary>
    /// Creates resilience options with sensible default settings optimized for Kontent.ai APIs.
    /// These defaults are based on real-world usage patterns and Kontent.ai SLA characteristics.
    /// </summary>
    /// <returns>Configured resilience options with production-ready defaults.</returns>
    public static ResilienceOptions CreateDefault()
    {
        return new ResilienceOptions
        {
            EnableRetry = true,
            EnableCircuitBreaker = true,
            EnableTimeout = true,
            Retry = new RetryOptions
            {
                MaxRetryAttempts = 3,
                BaseDelay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                UseExponentialBackoff = true
            },
            CircuitBreaker = new CircuitBreakerOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            },
            Timeout = new TimeoutOptions
            {
                Timeout = TimeSpan.FromSeconds(30)
            }
        };
    }
}

/// <summary>
/// Configuration options for retry behavior.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// </summary>
    public TimeSpan BaseDelay { get; set; }

    /// <summary>
    /// Gets or sets whether to use jitter in retry delays.
    /// </summary>
    public bool UseJitter { get; set; }

    /// <summary>
    /// Gets or sets whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; }
}

/// <summary>
/// Configuration options for circuit breaker behavior.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure ratio threshold that triggers the circuit breaker.
    /// </summary>
    public double FailureRatio { get; set; }

    /// <summary>
    /// Gets or sets the sampling duration for measuring failure ratio.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; }

    /// <summary>
    /// Gets or sets the minimum throughput required before circuit breaker evaluation.
    /// </summary>
    public int MinimumThroughput { get; set; }

    /// <summary>
    /// Gets or sets the duration to keep the circuit breaker open before attempting to close it.
    /// </summary>
    public TimeSpan BreakDuration { get; set; }
}

/// <summary>
/// Configuration options for timeout behavior.
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// Gets or sets the timeout duration for individual HTTP requests.
    /// </summary>
    public TimeSpan Timeout { get; set; }
}