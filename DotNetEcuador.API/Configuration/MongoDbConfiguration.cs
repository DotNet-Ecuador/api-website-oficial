using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Extensions;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Auth;
using MongoDB.Driver;

namespace DotNetEcuador.API.Configuration;

public static class MongoDbConfiguration
{
    public static void ConfigureMongoDB(this IServiceCollection services)
    {
        try
        {
            // Get MongoDB connection string from environment variable
            string mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "MONGO_CONNECTION_STRING environment variable is required. " +
                    "For local development, use: mongodb://localhost:27017/dotnet_ecuador");

            // Configure MongoDB Client with professional settings
            var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);
            mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(10);

            // Configure MongoDB Client and Database
            services.AddSingleton<IMongoClient>(sp =>
            {
                var logger = sp.GetService<ILogger<MongoClient>>();
                try
                {
                    var client = new MongoClient(mongoClientSettings);
                    // Test connection
                    client.GetDatabase(Constants.MONGODATABASE).RunCommand<object>("{ping:1}");
                    logger?.LogInformation("MongoDB connection established successfully to database: {Database}", Constants.MONGODATABASE);
                    return client;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to connect to MongoDB. Please ensure MongoDB is running on {ConnectionString}", mongoConnectionString);
                    throw new InvalidOperationException(
                        $"Could not connect to MongoDB at {mongoConnectionString}. " +
                        "Please ensure MongoDB is running locally without authentication. " +
                        "Start MongoDB with: mongod --dbpath <your-db-path>", ex);
                }
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(Constants.MONGODATABASE);
            });

            // Register repositories
            services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register MongoDB repositories
            services.AddMongoRepository<User>(Constants.MongoCollections.USERS);
            services.AddMongoRepository<AreaOfInterest>(Constants.MongoCollections.AREAINTEREST);
            services.AddMongoRepository<VolunteerApplication>(Constants.MongoCollections.VOLUNTEERAPPLICATION);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "MongoDB configuration failed. Please check your connection string and ensure MongoDB is running.", ex);
        }
    }
}
