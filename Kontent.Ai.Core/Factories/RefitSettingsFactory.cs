using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kontent.Ai.Core.Factories;

/// <summary>
/// Static helper factory that creates RefitSettings wired to System.Text.Json and shared content serializer.
/// Provides consistent JSON serialization settings across all Kontent.ai SDK clients.
/// </summary>
public static class RefitSettingsFactory
{
    /// <summary>
    /// Creates RefitSettings with System.Text.Json content serializer.
    /// </summary>
    /// <param name="jsonOptions">Optional custom JSON serialization options. If not provided, uses default options.</param>
    /// <returns>Configured RefitSettings instance.</returns>
    public static RefitSettings Create(JsonSerializerOptions? jsonOptions = null)
    {
        var options = jsonOptions ?? DefaultJsonOptions();
        
        return new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(options)
        };
    }

    /// <summary>
    /// Creates default JSON serialization options for Kontent.ai APIs.
    /// Uses camelCase property naming and ignores null values.
    /// </summary>
    /// <returns>Default JsonSerializerOptions for Kontent.ai APIs.</returns>
    public static JsonSerializerOptions DefaultJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Creates RefitSettings with custom System.Text.Json content serializer.
    /// </summary>
    /// <param name="contentSerializer">Custom System.Text.Json content serializer to use.</param>
    /// <returns>Configured RefitSettings instance.</returns>
    public static RefitSettings Create(SystemTextJsonContentSerializer contentSerializer)
    {
        ArgumentNullException.ThrowIfNull(contentSerializer);
        
        return new RefitSettings
        {
            ContentSerializer = contentSerializer
        };
    }
} 