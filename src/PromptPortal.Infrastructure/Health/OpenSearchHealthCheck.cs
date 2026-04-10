using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace PromptPortal.Infrastructure.Health;

public class OpenSearchHealthCheck(IOpenSearchClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var health = await client.Cluster.HealthAsync(h => h, ct);

            if (health.IsValid && health.Status is not OpenSearch.Client.Health.Red)
                return HealthCheckResult.Healthy($"OpenSearch cluster status: {health.Status}");

            return HealthCheckResult.Degraded($"OpenSearch cluster status: {health.Status}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("OpenSearch is unreachable", ex);
        }
    }
}
