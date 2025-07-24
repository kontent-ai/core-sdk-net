using System.Diagnostics;
using Kontent.Ai.Core.Attributes;
using Kontent.Ai.Core.Configuration;

namespace Kontent.Ai.Core.Extensions;

/// <summary>
/// Extension methods for adding headers to HTTP requests.
/// </summary>
public static class HttpRequestHeadersExtensions
{
    private const string SdkTrackingHeaderName = "X-KC-SDKID";
    private const string SourceTrackingHeaderName = "X-KC-SOURCE";
    private const string PackageRepositoryHost = "nuget.org";

    // Cache these values since they don't change during application lifetime
    private static readonly Lazy<string?> SourceTrackingHeaderValue = new(GetSourceTrackingHeaderValue);

    /// <summary>
    /// Adds the SDK tracking header to the request using the provided SDK identity.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    /// <param name="sdkIdentity">The SDK identity to use for tracking.</param>
    public static void AddSdkTrackingHeader(this HttpRequestHeaders headers, SdkIdentity sdkIdentity)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(sdkIdentity);

        var trackingValue = sdkIdentity.ToTrackingString(PackageRepositoryHost);
        headers.Add(SdkTrackingHeaderName, trackingValue);
    }

    /// <summary>
    /// Adds the source tracking header to the request according to Kontent.ai guidelines.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    public static void AddSourceTrackingHeader(this HttpRequestHeaders headers)
    {
        var source = SourceTrackingHeaderValue.Value;
        if (!string.IsNullOrEmpty(source))
            headers.Add(SourceTrackingHeaderName, source);
    }

    /// <summary>
    /// Adds authorization header with the specified scheme and parameter.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    /// <param name="scheme">The authorization scheme.</param>
    /// <param name="parameter">The authorization parameter.</param>
    public static void AddAuthorizationHeader(this HttpRequestHeaders headers, string scheme, string parameter) =>
        headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);

    /// <summary>
    /// Gets the product version from the assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get the version from.</param>
    /// <returns>The product version string.</returns>
    public static string GetProductVersion(this Assembly assembly)
    {
        string? version;
        if (string.IsNullOrEmpty(assembly.Location))
        {
            // Assembly.Location can be empty when publishing to a single file
            version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }
        else
        {
            try
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fileVersionInfo.ProductVersion;
            }
            catch (FileNotFoundException)
            {
                // Fallback for invalid assembly location paths
                version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            }
        }

        return version ?? "0.0.0";
    }

    private static string GenerateSourceTrackingHeaderValue(Assembly originatingAssembly, SourceTrackingHeaderAttribute attribute)
    {
        string packageName;
        string version;

        if (attribute.LoadFromAssembly)
        {
            packageName = attribute.PackageName ?? originatingAssembly.GetName().Name!;
            version = originatingAssembly.GetProductVersion();
        }
        else
        {
            packageName = attribute.PackageName!;
            string preRelease = string.IsNullOrEmpty(attribute.PreReleaseLabel) ? "" : $"-{attribute.PreReleaseLabel}";
            version = $"{attribute.MajorVersion}.{attribute.MinorVersion}.{attribute.PatchVersion}{preRelease}";
        }

        return $"{packageName};{version}";
    }

    private static string? GetSourceTrackingHeaderValue()
    {
        // Get the entry assembly (main application/tool)
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            // Fallback to process name if entry assembly is not available (some hosting scenarios)
            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                return $"{processName};unknown";
            }
            catch
            {
                return null;
            }
        }

        // Check if the entry assembly has a SourceTrackingHeaderAttribute
        var attribute = entryAssembly.GetCustomAttributes<SourceTrackingHeaderAttribute>().FirstOrDefault();
        if (attribute != null)
            return GenerateSourceTrackingHeaderValue(entryAssembly, attribute);

        // Fallback: use entry assembly name and version
        var assemblyName = entryAssembly.GetName().Name ?? "Unknown";
        var version = entryAssembly.GetProductVersion();
        return $"{assemblyName};{version}";
    }
}