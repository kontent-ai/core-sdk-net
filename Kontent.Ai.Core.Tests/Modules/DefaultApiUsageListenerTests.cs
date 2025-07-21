namespace Kontent.Ai.Core.Tests.Modules;

public class DefaultApiUsageListenerTests
{
    [Fact]
    public void Instance_IsNotNull()
    {
        // Act & Assert
        DefaultApiUsageListener.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Instance_ReturnsSameInstanceOnMultipleCalls()
    {
        // Act
        var first = DefaultApiUsageListener.Instance;
        var second = DefaultApiUsageListener.Instance;

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task OnRequestStartAsync_CompletesSuccessfully()
    {
        // Arrange
        var listener = DefaultApiUsageListener.Instance;
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.api/test");

        // Act
        var task = listener.OnRequestStartAsync(request);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task OnRequestEndAsync_CompletesSuccessfully()
    {
        // Arrange
        var listener = DefaultApiUsageListener.Instance;
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.api/test");
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var elapsed = TimeSpan.FromMilliseconds(100);

        // Act
        var task = listener.OnRequestEndAsync(request, response, null, elapsed);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }
} 