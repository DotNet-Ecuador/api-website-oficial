using DotNetEcuador.API.Infraestructure.Repositories;
using MongoDB.Driver;

namespace DotNetEcuador.API.Infraestructure.Extensions;
public static class RepositoryRegistrationExtensions
{
    public static void AddMongoRepository<T>(
        this IServiceCollection services,
        string collectionName)
        where T : class
    {
        services.AddScoped<IRepository<T>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return new Repository<T>(database, collectionName);
        });
    }
}
