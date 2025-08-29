using DotNetEcuador.API.Models.Common;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DotNetEcuador.API.Infraestructure.Extensions;

public static class MongoExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseWithFilterAsync<T>(
        this IMongoCollection<T> collection,
        PagedRequest request,
        FilterDefinition<T>? filter = null,
        SortDefinition<T>? sort = null)
    {
        filter ??= Builders<T>.Filter.Empty;

        var totalCount = await collection.CountDocumentsAsync(filter);

        var query = collection.Find(filter);

        if (sort != null)
        {
            query = query.Sort(sort);
        }

        var data = await query
            .Skip(request.Skip)
            .Limit(request.PageSize)
            .ToListAsync();

        return new PagedResponse<T>(data, (int)totalCount, request.Page, request.PageSize);
    }

    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IMongoCollection<T> collection,
        PagedRequest request,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? sortExpression = null,
        bool ascending = true)
    {
        try
        {
            var filterBuilder = Builders<T>.Filter;
            var filter = predicate != null ? filterBuilder.Where(predicate) : filterBuilder.Empty;

            var totalCount = await collection.CountDocumentsAsync(filter);

            var query = collection.Find(filter);

            if (sortExpression != null)
            {
                var sortBuilder = Builders<T>.Sort;
                var sort = ascending ? sortBuilder.Ascending(sortExpression) : sortBuilder.Descending(sortExpression);
                query = query.Sort(sort);
            }

            var data = await query
                .Skip(request.Skip)
                .Limit(request.PageSize)
                .ToListAsync();

            return new PagedResponse<T>(data, (int)totalCount, request.Page, request.PageSize);
        }
        catch (Exception)
        {
            // Si hay error, devolver respuesta vacía
            return new PagedResponse<T>(new List<T>(), 0, request.Page, request.PageSize);
        }
    }
}