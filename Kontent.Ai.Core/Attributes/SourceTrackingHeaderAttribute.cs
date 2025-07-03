namespace Kontent.Ai.Core.Attributes;

/// <summary>
/// Attribute for specifying source tracking header information for tools/applications that consume Kontent.ai SDKs.
/// This attribute should be applied to the assembly of the tool or application to enable proper tracking in the X-KC-SOURCE header.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class SourceTrackingHeaderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the SourceTrackingHeaderAttribute class.
    /// </summary>
    /// <param name="packageName">The package name.</param>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersion">The minor version.</param>
    /// <param name="patchVersion">The patch version.</param>
    public SourceTrackingHeaderAttribute(string packageName, int majorVersion, int minorVersion, int patchVersion)
    {
        PackageName = packageName;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        PatchVersion = patchVersion;
        LoadFromAssembly = false;
    }

    /// <summary>
    /// Initializes a new instance of the SourceTrackingHeaderAttribute class with assembly loading enabled.
    /// </summary>
    /// <param name="packageName">The package name (optional if loading from assembly).</param>
    public SourceTrackingHeaderAttribute(string? packageName = null)
    {
        PackageName = packageName;
        LoadFromAssembly = true;
    }

    /// <summary>
    /// Gets the package name.
    /// </summary>
    public string? PackageName { get; }

    /// <summary>
    /// Gets the major version.
    /// </summary>
    public int MajorVersion { get; }

    /// <summary>
    /// Gets the minor version.
    /// </summary>
    public int MinorVersion { get; }

    /// <summary>
    /// Gets the patch version.
    /// </summary>
    public int PatchVersion { get; }

    /// <summary>
    /// Gets or sets the pre-release label.
    /// </summary>
    public string? PreReleaseLabel { get; set; }

    /// <summary>
    /// Gets whether to load version information from the assembly.
    /// </summary>
    public bool LoadFromAssembly { get; }
} 