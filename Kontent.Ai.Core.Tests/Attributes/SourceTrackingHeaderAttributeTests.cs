namespace Kontent.Ai.Core.Tests.Attributes;

public class SourceTrackingHeaderAttributeTests
{
    [Fact]
    public void Constructor_WithVersionNumbers_SetsPropertiesCorrectly()
    {
        // Arrange
        const string packageName = "Test.Package";
        const int majorVersion = 1;
        const int minorVersion = 2;
        const int patchVersion = 3;

        // Act
        var attribute = new SourceTrackingHeaderAttribute(packageName, majorVersion, minorVersion, patchVersion);

        // Assert
        attribute.PackageName.Should().Be(packageName);
        attribute.MajorVersion.Should().Be(majorVersion);
        attribute.MinorVersion.Should().Be(minorVersion);
        attribute.PatchVersion.Should().Be(patchVersion);
        attribute.LoadFromAssembly.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithPackageName_SetsLoadFromAssemblyTrue()
    {
        // Arrange
        const string packageName = "Test.Package";

        // Act
        var attribute = new SourceTrackingHeaderAttribute(packageName);

        // Assert
        attribute.PackageName.Should().Be(packageName);
        attribute.LoadFromAssembly.Should().BeTrue();
    }
} 