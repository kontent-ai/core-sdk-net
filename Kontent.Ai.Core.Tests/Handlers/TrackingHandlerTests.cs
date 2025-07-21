namespace Kontent.Ai.Core.Tests.Handlers;

public class TrackingHandlerTests
{
    [Fact]
    public void Constructor_WithValidSdkIdentity_DoesNotThrow()
    {
        // Arrange
        var sdkIdentity = new SdkIdentity("Test.SDK", new Version(1, 0, 0));

        // Act & Assert
        var action = () => new TrackingHandler(sdkIdentity);
        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullSdkIdentity_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new TrackingHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }
} 