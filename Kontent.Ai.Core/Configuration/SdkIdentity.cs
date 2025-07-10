namespace Kontent.Ai.Core.Configuration;

/// <summary>
/// Represents the identity of an SDK package for tracking purposes.
/// </summary>
/// <param name="Name">The name of the SDK package (e.g., "Kontent.Ai.Delivery").</param>
/// <param name="Version">The version of the SDK package.</param>
public record SdkIdentity(string Name, Version Version)
{
    /// <summary>
    /// Creates an SDK identity for the Kontent.ai Core SDK.
    /// </summary>
    public static SdkIdentity Core { get; } = new("Kontent.Ai.Core", GetCoreVersion());

    /// <summary>
    /// Formats the SDK identity for use in tracking headers.
    /// </summary>
    /// <param name="packageRepositoryHost">The repository host (default: "nuget.org").</param>
    /// <returns>Formatted tracking string in the format "host;name;version".</returns>
    public string ToTrackingString(string packageRepositoryHost = "nuget.org") =>
        $"{packageRepositoryHost};{Name};{Version}";

    private static Version GetCoreVersion()
    {
        var assembly = typeof(SdkIdentity).Assembly;
        var versionAttribute = assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
        
        if (versionAttribute?.InformationalVersion != null &&
            Version.TryParse(versionAttribute.InformationalVersion.Split('-')[0], out var version))
        {
            return version;
        }
        
        return new Version(1, 0, 0);
    }
} 