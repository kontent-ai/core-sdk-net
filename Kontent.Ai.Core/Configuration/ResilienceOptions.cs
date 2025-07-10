namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Configuration options for resilience policies applied to Kontent.ai HTTP clients.
/// Provides sensible defaults for Kontent.ai APIs while allowing customization.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Gets or sets whether to enable the default resilience strategies.
    /// When false, no default policies are applied, allowing full customization.
    /// </summary>
    public bool EnableDefaultStrategies { get; set; } = true;

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
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable the circuit breaker strategy.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable the timeout strategy.
    /// </summary>
    public bool EnableTimeout { get; set; } = true;

    /// <summary>
    /// Creates resilience options from ClientOptions.
    /// </summary>
    /// <param name="clientOptions">The client options to derive resilience settings from.</param>
    /// <returns>Configured resilience options.</returns>
    public static ResilienceOptions FromClientOptions(ClientOptions clientOptions)
    {
        var options = new ResilienceOptions
        {
            EnableDefaultStrategies = clientOptions.EnableDefaultResilience
        };

        // Configure retry from client options
        options.Retry.MaxRetryAttempts = clientOptions.MaxRetryAttempts;
        options.Retry.BaseDelay = clientOptions.RetryBaseDelay;

        // Configure timeout from client options
        options.Timeout.Timeout = clientOptions.RequestTimeout;

        return options;
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
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to use jitter in retry delays.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}

/// <summary>
/// Configuration options for circuit breaker behavior.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure ratio threshold that triggers the circuit breaker.
    /// </summary>
    public double FailureRatio { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the sampling duration for measuring failure ratio.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the minimum throughput required before circuit breaker evaluation.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Gets or sets the duration to keep the circuit breaker open before attempting to close it.
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Configuration options for timeout behavior.
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// Gets or sets the timeout duration for individual HTTP requests.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
} 