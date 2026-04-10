using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Infrastructure.Persistence;

public class DynamoDbPromptRepository(IAmazonDynamoDB dynamoDb) : IPromptRepository
{
    private const string TableName = "PromptLibrary";

    public async Task<PromptDefinition?> GetByIdAsync(string tenantId, string promptId, CancellationToken ct = default)
    {
        var response = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new($"TENANT#{tenantId}"),
                ["SK"] = new($"PROMPT#{promptId}")
            }
        }, ct);

        if (!response.IsItemSet)
            return null;

        return PromptMapper.FromDynamoDb(response.Item);
    }

    public async Task<IReadOnlyList<PromptDefinition>> ListAsync(string tenantId, PromptFilter? filter = null, CancellationToken ct = default)
    {
        var request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "PK = :pk AND begins_with(SK, :skPrefix)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new($"TENANT#{tenantId}"),
                [":skPrefix"] = new("PROMPT#")
            }
        };

        var response = await dynamoDb.QueryAsync(request, ct);
        var prompts = response.Items.Select(PromptMapper.FromDynamoDb).ToList();

        if (filter?.Tag is not null)
            prompts = prompts.Where(p => p.Tags.Contains(filter.Tag, StringComparer.OrdinalIgnoreCase)).ToList();

        if (filter?.Category is not null)
            prompts = prompts.Where(p => p.Categories.Contains(filter.Category, StringComparer.OrdinalIgnoreCase)).ToList();

        if (filter?.Status is not null)
            prompts = prompts.Where(p => p.Status.ToString().Equals(filter.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        return prompts.AsReadOnly();
    }

    public async Task SaveAsync(PromptDefinition prompt, CancellationToken ct = default)
    {
        var item = PromptMapper.ToDynamoDb(prompt);

        await dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = item
        }, ct);
    }

    public async Task DeleteAsync(string tenantId, string promptId, CancellationToken ct = default)
    {
        await dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new($"TENANT#{tenantId}"),
                ["SK"] = new($"PROMPT#{promptId}")
            }
        }, ct);
    }
}
