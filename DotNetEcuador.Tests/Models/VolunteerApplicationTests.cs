using api.Models;
using FluentAssertions;

namespace DotNetEcuador.Tests.Models;

public class VolunteerApplicationTests
{
    [Fact]
    public void ValidateOtherAreas_ShouldReturnTrue_WhenOtherIsNotSelected()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true },
                { "ContentCreation", false }
            },
            OtherAreas = ""
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateOtherAreas_ShouldReturnTrue_WhenOtherIsSelectedAndOtherAreasHasValue()
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
    public void ValidateOtherAreas_ShouldReturnFalse_WhenOtherIsSelectedButOtherAreasIsEmpty()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "Other", true }
            },
            OtherAreas = ""
        };

        // Act
        var result = application.ValidateOtherAreas();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOtherAreas_ShouldReturnFalse_WhenOtherIsSelectedButOtherAreasIsWhitespace()
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
    public void ValidateOtherAreas_ShouldReturnTrue_WhenOtherKeyDoesNotExist()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true },
                { "ContentCreation", true }
            },
            OtherAreas = ""
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
    public void FullName_ShouldAcceptVariousInputs(string fullName)
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
    public void Email_ShouldAcceptVariousInputs(string email)
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