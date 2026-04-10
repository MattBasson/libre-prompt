using Amazon.DynamoDBv2;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PromptPortal.Infrastructure.Health;

public class DynamoDbHealthCheck(IAmazonDynamoDB dynamoDb) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await dynamoDb.ListTablesAsync(ct);
            return HealthCheckResult.Healthy("DynamoDB Local is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("DynamoDB Local is unreachable", ex);
        }
    }
}
