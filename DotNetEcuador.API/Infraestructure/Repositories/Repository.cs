using DotNetEcuador.API.Infraestructure.Extensions;
using DotNetEcuador.API.Models.Common;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DotNetEcuador.API.Infraestructure.Repositories;

public interface IRepository<T> : IReadRepository<T>
{
    Task CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FindOneAsync(FilterDefinition<T> filter);
}

public interface IReadRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<PagedResponse<T>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<T>> GetPagedAsync(PagedRequest request, Expression<Func<T, bool>> predicate);
}

public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly IMongoCollection<T> _collection;
    private readonly string _collectionName;

    public Repository(
        IMongoDatabase database,
        string collectionName = "")
    {
        _collectionName = collectionName;
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<List<T>> GetAllAsync()
    => await _collection.Find(_ => true).ToListAsync().ConfigureAwait(false);

    public async Task<List<T>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _collection.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task CreateAsync(T entity)
    {
        try
        {
            Console.WriteLine($"[Repository] Attempting to insert into collection: {_collectionName}");
            await _collection.InsertOneAsync(entity).ConfigureAwait(false);
            Console.WriteLine($"[Repository] Successfully inserted into collection: {_collectionName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Repository] Failed to insert into collection {_collectionName}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(string id, T entity)
    {
        await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id)).ConfigureAwait(false);
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<T?> FindOneAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<PagedResponse<T>> GetPagedAsync(PagedRequest request)
    {
        return await _collection.ToPagedResponseAsync(request, null, null, true);
    }

    public async Task<PagedResponse<T>> GetPagedAsync(PagedRequest request, Expression<Func<T, bool>> predicate)
    {
        return await _collection.ToPagedResponseAsync(request, predicate);
    }
}
