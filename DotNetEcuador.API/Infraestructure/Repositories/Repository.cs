using MongoDB.Driver;

namespace DotNetEcuador.API.Infraestructure.Repositories;

public interface IRepository<T> : IReadRepository<T>
{
	Task CreateAsync(T entity);
	Task UpdateAsync(string id, T entity);
	Task DeleteAsync(string id);
}

public interface IReadRepository<T>
{
	Task<List<T>> GetAllAsync();
	Task<T?> GetByIdAsync(string id);
}

public class Repository<T> : IRepository<T> where T : class
{
	private readonly IMongoCollection<T> _collection;

	public Repository(
		IMongoDatabase database,
		string collectionName = "")
	{
		_collection = database.GetCollection<T>(collectionName);
	}

	public async Task<List<T>> GetAllAsync()
	=> await _collection.Find(_ => true).ToListAsync();

	public async Task<List<T>> GetAllAsync(int pageNumber, int pageSize)
	{	
		return await _collection.Find(_ => true)
			.Skip((pageNumber - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync();
	}

	public async Task<T?> GetByIdAsync(string id)
	{
		return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
	}

	public async Task CreateAsync(T entity)
	{
		await _collection.InsertOneAsync(entity);
	}

	public async Task UpdateAsync(string id, T entity)
	{
		await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);
	}

	public async Task DeleteAsync(string id)
	{
		await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
	}
}
