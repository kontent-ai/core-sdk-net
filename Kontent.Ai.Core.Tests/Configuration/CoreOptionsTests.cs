namespace Kontent.Ai.Core.Tests.Configuration;

public class CoreOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var options = new CoreOptions();

        // Assert
        options.TelemetryExceptionBehavior.Should().Be(TelemetryExceptionBehavior.LogAndContinue);
    }

    [Fact]
    public void CreateDefaultRefitSettings_ReturnsValidSettings()
    {
        // Act
        var settings = CoreOptions.CreateDefaultRefitSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.ContentSerializer.Should().NotBeNull();
    }
}