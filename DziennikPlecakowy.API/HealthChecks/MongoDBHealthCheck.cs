using Microsoft.Extensions.Diagnostics.HealthChecks;
using DziennikPlecakowy.Interfaces;

namespace DziennikPlecakowy.API.HealthChecks;

public class MongoDBHealthCheck : IHealthCheck
{
    private readonly IMongoDbContext _dbContext;

    public MongoDBHealthCheck(IMongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Próba wykonania prostej, niewymagającej zasobów operacji na bazie
            await _dbContext.Users.EstimatedDocumentCountAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Połączenie z MongoDB jest sprawne.");
        }
        catch (Exception ex)
        {
            // Jeśli wystąpi błąd (np. brak połączenia), zwracamy status Unhealthy
            return HealthCheckResult.Unhealthy($"Połączenie z MongoDB nie działa: {ex.Message}");
        }
    }
}