using api.Models;
using DotNetEcuador.API.Infraestructure.Repositories;
using FluentAssertions;
using MongoDB.Driver;
using Moq;

namespace DotNetEcuador.Tests.Repositories;

public class RepositoryTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<AreaOfInterest>> _mockCollection;
    private readonly Repository<AreaOfInterest> _repository;

    public RepositoryTests()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<AreaOfInterest>>();
        
        _mockDatabase
            .Setup(db => db.GetCollection<AreaOfInterest>("test_collection", null))
            .Returns(_mockCollection.Object);
        
        _repository = new Repository<AreaOfInterest>(_mockDatabase.Object, "test_collection");
    }

    // Note: GetAllAsync and GetByIdAsync tests are complex to mock with MongoDB extension methods
    // These would be better tested with integration tests using a test database

    [Fact]
    public async Task CreateAsync_ShouldCallInsertOneAsync_WhenEntityIsValid()
    {
        // Arrange
        var entity = new AreaOfInterest 
        { 
            Id = "1", 
            Name = "Test", 
            Description = "Description" 
        };

        // Act
        await _repository.CreateAsync(entity);

        // Assert
        _mockCollection.Verify(
            collection => collection.InsertOneAsync(entity, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallReplaceOneAsync_WhenEntityExists()
    {
        // Arrange
        var entity = new AreaOfInterest 
        { 
            Id = "1", 
            Name = "Updated", 
            Description = "Updated Description" 
        };

        // Act
        await _repository.UpdateAsync("1", entity);

        // Assert
        _mockCollection.Verify(
            collection => collection.ReplaceOneAsync(
                It.IsAny<FilterDefinition<AreaOfInterest>>(),
                entity,
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteOneAsync_WhenIdIsProvided()
    {
        // Arrange
        const string entityId = "1";

        // Act
        await _repository.DeleteAsync(entityId);

        // Assert
        _mockCollection.Verify(
            collection => collection.DeleteOneAsync(
                It.IsAny<FilterDefinition<AreaOfInterest>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}