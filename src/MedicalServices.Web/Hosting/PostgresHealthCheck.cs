using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MedicalServices.Web.Hosting;

public sealed class PostgresHealthCheck(IConfiguration config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var cs = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs))
            return HealthCheckResult.Unhealthy("Missing connection string");

        try
        {
            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Equals(result, 1) || Equals(result, 1L)
                ? HealthCheckResult.Healthy("PostgreSQL reachable")
                : HealthCheckResult.Degraded("Unexpected ping result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL unreachable", ex);
        }
    }
}
