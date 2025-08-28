using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using FluentAssertions;
using MongoDB.Driver;
using Moq;

namespace DotNetEcuador.Tests.Services;

public class VolunteerApplicationServiceTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<VolunteerApplication>> _mockCollection;
    private readonly IVolunteerApplicationService _service;

    public VolunteerApplicationServiceTests()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<VolunteerApplication>>();

        _mockDatabase
            .Setup(db => db.GetCollection<VolunteerApplication>("volunteer_applications", null))
            .Returns(_mockCollection.Object);

        _service = new VolunteerApplicationService(_mockDatabase.Object);
    }

    [Fact]
    public async Task CreateAsyncShouldCallRepositoryCreateWhenApplicationIsValid()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            City = "Quito",
            HasVolunteeringExperience = true,
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true },
                { "ContentCreation", false }
            },
            AvailableTime = "Weekends",
            SkillsOrKnowledge = "Programming, Marketing",
            WhyVolunteer = "Want to contribute to the community",
            AdditionalComments = "Available immediately"
        };

        // Act
        await _service.CreateAsync(application).ConfigureAwait(false);

        // Assert
        _mockCollection.Verify(
            collection => collection.InsertOneAsync(application, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("EventOrganization", true)]
    [InlineData("ContentCreation", true)]
    [InlineData("TechnicalSupport", true)]
    [InlineData("SocialMediaManagement", true)]
    [InlineData("Other", true)]
    [InlineData("InvalidArea", false)]
    [InlineData("", false)]
    public void AreValidAreasOfInterestShouldValidateAreaNamesCorrectly(string areaName, bool expectedValid)
    {
        // Arrange
        var areasOfInterest = new Dictionary<string, bool>
        {
            { areaName, true }
        };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnTrueWhenAllAreasAreValid()
    {
        // Arrange
        var areasOfInterest = new Dictionary<string, bool>
        {
            { "EventOrganization", true },
            { "ContentCreation", false },
            { "TechnicalSupport", true }
        };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnFalseWhenAnyAreaIsInvalid()
    {
        // Arrange
        var areasOfInterest = new Dictionary<string, bool>
        {
            { "EventOrganization", true },
            { "InvalidArea", true },
            { "TechnicalSupport", false }
        };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnTrueWhenDictionaryIsEmpty()
    {
        // Arrange
        var areasOfInterest = new Dictionary<string, bool>();

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeTrue();
    }
}
