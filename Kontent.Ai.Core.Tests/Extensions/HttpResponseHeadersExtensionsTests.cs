namespace Kontent.Ai.Core.Tests.Extensions;

public class HttpResponseHeadersExtensionsTests
{
    [Fact]
    public void GetContinuationHeader_WithExistingHeader_ReturnsValue()
    {
        // Arrange
        using var response = new HttpResponseMessage();
        const string expectedToken = "test-continuation-token";
        response.Headers.Add("X-Continuation", expectedToken);

        // Act
        var result = response.Headers.GetContinuationHeader();

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public void TryGetRetryAfter_WithDeltaHeader_ReturnsCorrectTimeSpan()
    {
        // Arrange
        using var response = new HttpResponseMessage();
        var expectedDelay = TimeSpan.FromSeconds(30);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(expectedDelay);

        // Act
        var success = response.Headers.TryGetRetryAfter(out var retryAfter);

        // Assert
        success.Should().BeTrue();
        retryAfter.Should().Be(expectedDelay);
    }
} 