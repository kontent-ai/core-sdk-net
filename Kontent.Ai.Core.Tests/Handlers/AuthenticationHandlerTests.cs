namespace Kontent.Ai.Core.Tests.Handlers;

public class AuthenticationHandlerTests
{
    [Fact]
    public void Constructor_WithNullOptionsMonitor_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new AuthenticationHandler<TestClientOptions>(null!);
        action.Should().Throw<ArgumentNullException>();
    }
} 