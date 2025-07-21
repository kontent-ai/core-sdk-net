namespace Kontent.Ai.Core.Tests.Extensions;

public class HttpRequestHeadersExtensionsTests
{
    [Fact]
    public void AddSdkTrackingHeader_AddsCorrectHeader()
    {
        // Arrange
        using var request = new HttpRequestMessage();
        var sdkIdentity = new SdkIdentity("Test.SDK", new Version(1, 2, 3));

        // Act
        request.Headers.AddSdkTrackingHeader(sdkIdentity);

        // Assert
        request.Headers.Should().ContainKey("X-KC-SDKID");
        var headerValue = request.Headers.GetValues("X-KC-SDKID").First();
        headerValue.Should().Be("nuget.org;Test.SDK;1.2.3");
    }

    [Fact]
    public void AddAuthorizationHeader_SetsCorrectHeader()
    {
        // Arrange
        using var request = new HttpRequestMessage();

        // Act
        request.Headers.AddAuthorizationHeader("Bearer", "test-token");

        // Assert
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("test-token");
    }
} 