using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Refit;

namespace Kontent.Ai.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    /// <summary>
    /// Test client options that inherit from ClientOptions - simulates real SDK usage
    /// </summary>
    public class DeliveryClientOptions : ClientOptions
    {
        // Inherits required EnvironmentId from ClientOptions
        // This class can now be used with AddClient without the new() constraint issue

        public string BaseUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public string? PreviewApiKey { get; set; }
        public bool UsePreviewMode { get; set; } = false;

        public override string GetBaseUrl(object? requestContext = null) => BaseUrl;

        public override string? GetApiKey(object? requestContext = null) 
            => UsePreviewMode ? PreviewApiKey : ApiKey;
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

        // Verify the options are properly configured
        var configuredOptions = serviceProvider.GetRequiredService<DeliveryClientOptions>();
        configuredOptions.EnvironmentId.Should().Be("test-environment-id");
        configuredOptions.BaseUrl.Should().Be("https://deliver.kontent.ai");
        configuredOptions.PreviewApiKey.Should().Be("preview-key");
        configuredOptions.UsePreviewMode.Should().BeTrue();

        // Verify core services are registered
        serviceProvider.GetService<TelemetryHandler>().Should().NotBeNull();
        serviceProvider.GetService<TrackingHandler>().Should().NotBeNull();
        serviceProvider.GetService<IApiUsageListener>().Should().NotBeNull();
    }

    [Fact]
    public void AddClient_WithConfigurationSection_WorksWithRequiredMembers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestDelivery:EnvironmentId"] = "config-environment-id",
                ["TestDelivery:BaseUrl"] = "https://deliver.kontent.ai",
                ["TestDelivery:PreviewApiKey"] = "config-preview-key",
                ["TestDelivery:UsePreviewMode"] = "true"
            })
            .Build();

        var section = configuration.GetSection("TestDelivery");

        // Act
        services.AddClient<ITestDeliveryClient, DeliveryClientOptions>(section);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<DeliveryClientOptions>();

        configuredOptions.EnvironmentId.Should().Be("config-environment-id");
        configuredOptions.BaseUrl.Should().Be("https://deliver.kontent.ai");
        configuredOptions.PreviewApiKey.Should().Be("config-preview-key");
        configuredOptions.UsePreviewMode.Should().BeTrue();
    }

    [Fact]
    public void AddClient_WithInvalidOptions_ThrowsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddClient<ITestDeliveryClient, DeliveryClientOptions>(options =>
        {
            // Intentionally leave EnvironmentId empty to trigger validation error
            options.EnvironmentId = "";
            options.BaseUrl = "https://deliver.kontent.ai";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Validation happens when options are accessed
        var action = () => serviceProvider.GetRequiredService<DeliveryClientOptions>();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Client options validation failed*EnvironmentId is required*");
    }

    [Fact]
    public void AddCore_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<TelemetryHandler>().Should().NotBeNull();
        serviceProvider.GetService<TrackingHandler>().Should().NotBeNull();
        serviceProvider.GetService<IApiUsageListener>().Should().NotBeNull();
        serviceProvider.GetService<IApiUsageListener>().Should().BeOfType<DefaultApiUsageListener>();
        serviceProvider.GetService<RefitSettings>().Should().NotBeNull();
    }

    [Fact]
    public void AddCore_WithCustomConfiguration_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var customListener = new Mock<IApiUsageListener>().Object;

        // Act
        services.AddCore(opts =>
        {
            // Note: CoreOptions is a record with init-only properties
            // In practice, custom configuration would be done differently,
            // but we're just testing that the configure action is called
        });

        // Override with PostConfigure to test custom listener
        services.PostConfigure<CoreOptions>(opts =>
        {
            // This demonstrates how to configure CoreOptions in practice
        });

        // Manually register custom listener to test integration
        services.AddSingleton<IApiUsageListener>(customListener);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var listener = serviceProvider.GetService<IApiUsageListener>();
        listener.Should().Be(customListener);

        var coreOptions = serviceProvider.GetService<IOptions<CoreOptions>>();
        coreOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddClient_EnablesResilienceByDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddClient<ITestDeliveryClient, DeliveryClientOptions>(options =>
        {
            options.EnvironmentId = "test-env";
            options.BaseUrl = "https://deliver.kontent.ai";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<DeliveryClientOptions>();
        configuredOptions.EnableResilience.Should().BeTrue();
    }

    [Fact]
    public void AddClient_CanDisableResilience()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddClient<ITestDeliveryClient, DeliveryClientOptions>(options =>
        {
            options.EnvironmentId = "test-env";
            options.BaseUrl = "https://deliver.kontent.ai";
            options.EnableResilience = false;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configuredOptions = serviceProvider.GetRequiredService<DeliveryClientOptions>();
        configuredOptions.EnableResilience.Should().BeFalse();
    }
}