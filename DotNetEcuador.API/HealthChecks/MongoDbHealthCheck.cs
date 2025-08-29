using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace DotNetEcuador.API.HealthChecks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbHealthCheck> _logger;

    public MongoDbHealthCheck(IMongoDatabase database, ILogger<MongoDbHealthCheck> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ping the database
            await _database.RunCommandAsync<object>("{ping:1}", cancellationToken: cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "database", _database.DatabaseNamespace.DatabaseName },
                { "status", "Connected" }
            };

            _logger.LogDebug("MongoDB health check passed");
            return HealthCheckResult.Healthy("MongoDB is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            
            var data = new Dictionary<string, object>
            {
                { "database", _database.DatabaseNamespace.DatabaseName },
                { "status", "Disconnected" },
                { "error", ex.Message }
            };

            return HealthCheckResult.Unhealthy("MongoDB is unhealthy", ex, data);
        }
    }
}