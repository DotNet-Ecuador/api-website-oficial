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
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true },
                { "ContentCreation", false }
            },
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
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "Other", true }
            },
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
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "Other", true }
            },
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
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "Other", true }
            },
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
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true },
                { "ContentCreation", true }
            },
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
