namespace Kontent.Ai.Core.Tests.Configuration;

public class SdkIdentityTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        const string name = "Test.SDK";
        var version = new Version(1, 2, 3);

        // Act
        var identity = new SdkIdentity(name, version);

        // Assert
        identity.Name.Should().Be(name);
        identity.Version.Should().Be(version);
    }

    [Fact]
    public void Core_ReturnsValidIdentity()
    {
        // Act
        var coreIdentity = SdkIdentity.Core;

        // Assert
        coreIdentity.Should().NotBeNull();
        coreIdentity.Name.Should().Be("Kontent.Ai.Core");
        coreIdentity.Version.Should().NotBeNull();
    }

    [Theory]
    [InlineData("nuget.org", "Test.SDK", "1.0.0", "nuget.org;Test.SDK;1.0.0")]
    [InlineData("custom.host", "My.Package", "2.1.5", "custom.host;My.Package;2.1.5")]
    public void ToTrackingString_ReturnsCorrectFormat(string host, string name, string versionString, string expected)
    {
        // Arrange
        var version = Version.Parse(versionString);
        var identity = new SdkIdentity(name, version);

        // Act
        var result = identity.ToTrackingString(host);

        // Assert
        result.Should().Be(expected);
    }
}