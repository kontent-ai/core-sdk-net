using Kontent.Ai.Core.Abstractions;
using System.Text.Json;

namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Configuration options for Kontent.ai core services registration.
/// </summary>
/// <param name="ConfigureJsonOptions">Optional action to configure custom JSON serialization options.</param>
/// <param name="ApiUsageListener">Optional custom API usage listener for telemetry.</param>
/// <param name="SdkIdentity">Optional SDK identity for tracking headers. If not provided, uses SdkIdentity.Core.</param>
/// <param name="TelemetryExceptionBehavior">Configures how telemetry exceptions are handled. Default is LogAndContinue.</param>
public record CoreServicesOptions(
    Action<JsonSerializerOptions>? ConfigureJsonOptions = null,
    IApiUsageListener? ApiUsageListener = null,
    SdkIdentity? SdkIdentity = null,
    TelemetryExceptionBehavior TelemetryExceptionBehavior = TelemetryExceptionBehavior.LogAndContinue
);

/// <summary>
/// Defines how telemetry exceptions should be handled.
/// </summary>
public enum TelemetryExceptionBehavior
{
    /// <summary>
    /// Log the exception and continue processing (default behavior).
    /// Recommended for production scenarios where telemetry failures shouldn't impact the main request.
    /// </summary>
    LogAndContinue,

    /// <summary>
    /// Throw the exception and fail the request.
    /// Recommended for development scenarios where you want to catch configuration issues early.
    /// </summary>
    ThrowException
}