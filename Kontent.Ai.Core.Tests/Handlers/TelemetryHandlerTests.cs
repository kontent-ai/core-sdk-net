namespace Kontent.Ai.Core.Tests.Handlers;

public class TelemetryHandlerTests
{
    [Fact]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Arrange
        var listener = new Mock<IApiUsageListener>();
        var logger = new Mock<ILogger<TelemetryHandler>>();
        var coreOptions = Microsoft.Extensions.Options.Options.Create(new CoreOptions());

        // Act & Assert
        var action = () => new TelemetryHandler(listener.Object, logger.Object, coreOptions);
        action.Should().NotThrow();
    }
}