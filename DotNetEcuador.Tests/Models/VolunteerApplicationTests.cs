using DotNetEcuador.API.Models;
using FluentAssertions;

namespace DotNetEcuador.Tests.Models;

public class VolunteerApplicationTests
{
    [Fact]
    public void ValidateOtherAreasShouldReturnTrueWhenOtherIsNotSelected()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new List<string> { "EventOrganization" },
            OtherAreas = string.Empty
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateOtherAreasShouldReturnTrueWhenOtherIsSelectedAndOtherAreasHasValue()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new List<string> { "Other" },
            OtherAreas = "Custom area of interest"
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateOtherAreasShouldReturnFalseWhenOtherIsSelectedButOtherAreasIsEmpty()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new List<string> { "Other" },
            OtherAreas = string.Empty
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOtherAreasShouldReturnFalseWhenOtherIsSelectedButOtherAreasIsWhitespace()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new List<string> { "Other" },
            OtherAreas = "   "
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOtherAreasShouldReturnTrueWhenOtherKeyDoesNotExist()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new List<string> { "EventOrganization", "ContentCreation" },
            OtherAreas = string.Empty
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("John Doe")]
    [InlineData("María García")]
    public void FullNameShouldAcceptVariousInputs(string fullName)
    {
        // Arrange & Act
        var application = new VolunteerApplication
        {
            FullName = fullName
        };

        // Assert
        application.FullName.Should().Be(fullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.com")]
    public void EmailShouldAcceptVariousInputs(string email)
    {
        // Arrange & Act
        var application = new VolunteerApplication
        {
            Email = email
        };

        // Assert
        application.Email.Should().Be(email);
    }
}
