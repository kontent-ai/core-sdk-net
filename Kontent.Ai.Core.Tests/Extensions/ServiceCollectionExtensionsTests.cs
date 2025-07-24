using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Kontent.Ai.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    /// <summary>
    /// Test client options that inherit from ClientOptions - simulates real SDK usage
    /// </summary>
    public record DeliveryClientOptions : ClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public string? PreviewApiKey { get; set; }
        public bool UsePreviewMode { get; set; } = false;
    }

    public interface ITestDeliveryClient
    {
        [Get("/items")]
        Task<string> GetItemsAsync();
    }

    [Fact]
    public void AddClient_WithRequiredMembers_CompilesAndRegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - This should compile without issues now that new() constraint is removed
        services.AddClient<ITestDeliveryClient, DeliveryClientOptions>(options =>
        {
            options.EnvironmentId = "test-environment-id";
            options.BaseUrl = "https://deliver.kontent.ai";
            options.PreviewApiKey = "preview-key";
            options.UsePreviewMode = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<DeliveryClientOptions>();
        configuredOptions.EnvironmentId.Should().Be("test-environment-id");
        configuredOptions.BaseUrl.Should().Be("https://deliver.kontent.ai");
    }
}

/// <summary>
/// Extension methods for DeliveryClientOptions to provide base URL and API key resolution.
/// </summary>
public static class DeliveryClientOptionsExtensions
{
    /// <summary>
    /// Gets the base URL for delivery client options.
    /// </summary>
    public static string GetBaseUrl(this ServiceCollectionExtensionsTests.DeliveryClientOptions options, object? requestContext = null) 
        => options.BaseUrl;

    /// <summary>
    /// Gets the API key for delivery client options.
    /// </summary>
    public static string? GetApiKey(this ServiceCollectionExtensionsTests.DeliveryClientOptions options, object? requestContext = null) 
        => options.UsePreviewMode ? options.PreviewApiKey : options.ApiKey;
}
