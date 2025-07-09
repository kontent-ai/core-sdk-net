using System.Collections.Concurrent;
using System.Diagnostics;
using Kontent.Ai.Core.Attributes;

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
    private static readonly ConcurrentDictionary<Assembly, string> SdkTrackingHeaderCache = [];
    private static readonly Lazy<string?> SourceTrackingHeaderValue = new(GetSourceTrackingHeaderValue);

    /// <summary>
    /// Adds the SDK tracking header to the request.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    /// <param name="sdkAssembly">Optional SDK assembly to use for tracking. If not provided, attempts to detect automatically.</param>
    public static void AddSdkTrackingHeader(this HttpRequestHeaders headers, Assembly? sdkAssembly = null)
    {
        sdkAssembly ??= GetSdkAssembly();
        if (sdkAssembly != null)
        {
            var trackingValue = SdkTrackingHeaderCache.GetOrAdd(sdkAssembly, assembly =>
            {
                var sdkVersion = assembly.GetProductVersion();
                var sdkPackageId = assembly.GetName().Name;
                return $"{PackageRepositoryHost};{sdkPackageId};{sdkVersion}";
            });
            
            headers.Add(SdkTrackingHeaderName, trackingValue);
        }
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

    private static Assembly? GetSdkAssembly()
    {
        // Fallback method when SDK assembly is not explicitly provided
        // This should rarely be used since TrackingHandler now receives the assembly directly
        var callingAssembly = Assembly.GetCallingAssembly();
        var executingAssembly = Assembly.GetExecutingAssembly(); // This is Kontent.Ai.Core
        
        // If the calling assembly is not the core package and references it, it's likely the SDK
        if (callingAssembly != executingAssembly &&
            callingAssembly.GetReferencedAssemblies()
                .Any(refAssembly => refAssembly.FullName == executingAssembly.FullName))
        {
            return callingAssembly;
        }
        
        return null;
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