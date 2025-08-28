using DotNetEcuador.API.Models;
using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Extensions;
using DotNetEcuador.API.Infraestructure.Repositories;
using MongoDB.Driver;

namespace DotNetEcuador.API.Configuration;

public static class MongoDbConfiguration
{
    public static void ConfigureMongoDB(this IServiceCollection services)
    {
        // Get MongoDB connection string from environment variable
        string mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "MONGO_CONNECTION_STRING environment variable is required. " +
                "Please set it in your system environment or systemd service configuration.");

        // Configure MongoDB Client and Database
        services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(Constants.MONGODATABASE);
        });

        // Register repositories
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register MongoDB repository for AreaOfInterest
        services.AddMongoRepository<AreaOfInterest>(Constants.MongoCollections.AREAINTEREST);
    }
}
