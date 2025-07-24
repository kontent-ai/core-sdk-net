using Kontent.Ai.Core.Abstractions;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Configuration options for Kontent.ai core services registration.
/// </summary>
/// <param name="ApiUsageListener">Optional custom API usage listener for telemetry.</param>
/// <param name="SdkIdentity">Optional SDK identity for tracking headers. If not provided, uses SdkIdentity.Core.</param>
/// <param name="TelemetryExceptionBehavior">Configures how telemetry exceptions are handled. Default is LogAndContinue.</param>
public record CoreOptions(
    IApiUsageListener? ApiUsageListener = null,
    SdkIdentity? SdkIdentity = null,
    TelemetryExceptionBehavior TelemetryExceptionBehavior = TelemetryExceptionBehavior.LogAndContinue
)
{
    /// <summary>
    /// Parameterless constructor for Microsoft.Extensions.Options compatibility.
    /// </summary>
    public CoreOptions() : this(null, null, TelemetryExceptionBehavior.LogAndContinue)
    {
    }
    /// <summary>
    /// Creates default RefitSettings optimized for Kontent.ai APIs.
    /// Uses camelCase property naming, ignores null values, and enables other JSON features.
    /// </summary>
    /// <returns>Default RefitSettings configured for Kontent.ai APIs.</returns>
    public static RefitSettings CreateDefaultRefitSettings()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };
    }
};

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