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
    public static SdkIdentity Core => _coreIdentity.Value;

    /// <summary>
    /// Formats the SDK identity for use in tracking headers.
    /// </summary>
    /// <param name="packageRepositoryHost">The repository host (default: "nuget.org").</param>
    /// <returns>Formatted tracking string in the format "host;name;version".</returns>
    public string ToTrackingString(string packageRepositoryHost = "nuget.org") =>
        $"{packageRepositoryHost};{Name};{Version}";

    private static readonly Lazy<SdkIdentity> _coreIdentity = new(() =>
        new("Kontent.Ai.Core", GetCoreVersion()));

    private static readonly Lazy<Version> _coreVersion = new(() =>
    {
        var assembly = typeof(SdkIdentity).Assembly;
        var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (versionAttribute?.InformationalVersion != null)
        {
            // Handle semantic versions with pre-release suffixes (e.g., "1.0.0-beta.1")
            var versionPart = versionAttribute.InformationalVersion.Split('-')[0];
            if (Version.TryParse(versionPart, out var version))
            {
                return version;
            }
        }

        // Fallback to assembly version if informational version is not available
        return assembly.GetName().Version ?? new Version(1, 0, 0);
    });

    private static Version GetCoreVersion() => _coreVersion.Value;
}