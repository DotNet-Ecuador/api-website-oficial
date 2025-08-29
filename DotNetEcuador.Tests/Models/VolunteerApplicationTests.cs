using DotNetEcuador.API.Models;
using FluentAssertions;

namespace DotNetEcuador.Tests.Models;

public class VolunteerApplicationTests
{
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

    [Fact]
    public void AreasOfInterestShouldInitializeAsEmptyList()
    {
        // Arrange & Act
        var application = new VolunteerApplication();

        // Assert
        application.AreasOfInterest.Should().NotBeNull();
        application.AreasOfInterest.Should().BeEmpty();
    }

    [Fact]
    public void ShouldAcceptMultipleAreasOfInterest()
    {
        // Arrange
        var areas = new List<string> { "EventOrganization", "TechnicalSupport", "Other" };

        // Act
        var application = new VolunteerApplication
        {
            AreasOfInterest = areas
        };

        // Assert
        application.AreasOfInterest.Should().BeEquivalentTo(areas);
    }
}