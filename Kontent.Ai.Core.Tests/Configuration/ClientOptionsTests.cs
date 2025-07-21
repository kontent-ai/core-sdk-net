namespace Kontent.Ai.Core.Tests.Configuration;

public class ClientOptionsTests
{
    [Fact]
    public void Validate_WithValidEnvironmentId_DoesNotThrow()
    {
        // Arrange
        var options = new TestClientOptions { EnvironmentId = "test-env", BaseUrl = "https://test.api" };

        // Act & Assert
        var action = () => options.Validate();
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidEnvironmentId_ThrowsInvalidOperationException(string environmentId)
    {
        // Arrange
        var options = new TestClientOptions { EnvironmentId = environmentId, BaseUrl = "https://test.api" };

        // Act & Assert
        var action = () => options.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("EnvironmentId is required");
    }

    [Fact]
    public void EnableResilience_DefaultsToTrue()
    {
        // Arrange & Act
        var options = new TestClientOptions { EnvironmentId = "test-env", BaseUrl = "https://test.api" };

        // Assert
        options.EnableResilience.Should().BeTrue();
    }
} 