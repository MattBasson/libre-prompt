using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PromptPortal.Infrastructure.Persistence;

public class DynamoDbTableInitializer(
    IAmazonDynamoDB dynamoDb,
    ILogger<DynamoDbTableInitializer> logger
) : IHostedService
{
    private const string TableName = "PromptLibrary";

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            var tables = await dynamoDb.ListTablesAsync(ct);
            if (tables.TableNames.Contains(TableName))
            {
                logger.LogInformation("DynamoDB table '{TableName}' already exists", TableName);
                return;
            }

            logger.LogInformation("Creating DynamoDB table '{TableName}'...", TableName);

            await dynamoDb.CreateTableAsync(new CreateTableRequest
            {
                TableName = TableName,
                KeySchema =
                [
                    new KeySchemaElement("PK", KeyType.HASH),
                    new KeySchemaElement("SK", KeyType.RANGE)
                ],
                AttributeDefinitions =
                [
                    new AttributeDefinition("PK", ScalarAttributeType.S),
                    new AttributeDefinition("SK", ScalarAttributeType.S)
                ],
                BillingMode = BillingMode.PAY_PER_REQUEST
            }, ct);

            logger.LogInformation("DynamoDB table '{TableName}' created successfully", TableName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize DynamoDB table '{TableName}'", TableName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
