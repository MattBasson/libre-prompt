using OpenSearch.Client;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Infrastructure.Search;

public class OpenSearchPromptIndexer(IOpenSearchClient client) : IPromptSearchIndexer
{
    private const string IndexName = "prompts-v1";

    public async Task IndexAsync(PromptDefinition prompt, CancellationToken ct = default)
    {
        var doc = new OpenSearchSearchDocument
        {
            PromptId = prompt.PromptId,
            TenantId = prompt.TenantId,
            Title = prompt.Title,
            Summary = prompt.Summary,
            Content = prompt.Content,
            Tags = prompt.Tags,
            Categories = prompt.Categories,
            Models = prompt.Models,
            Status = prompt.Status.ToString(),
            CreatedBy = prompt.CreatedBy,
            CreatedUtc = prompt.CreatedUtc,
            UpdatedUtc = prompt.UpdatedUtc
        };

        var documentId = $"{prompt.TenantId}_{prompt.PromptId}";

        await client.IndexAsync(doc, i => i
            .Index(IndexName)
            .Id(documentId)
            .Refresh(OpenSearch.Client.Refresh.WaitFor),
        ct);
    }

    public async Task RemoveAsync(string promptId, CancellationToken ct = default)
    {
        await client.DeleteByQueryAsync<OpenSearchSearchDocument>(d => d
            .Index(IndexName)
            .Query(q => q
                .Term(t => t.Field(f => f.PromptId).Value(promptId))
            ),
        ct);
    }
}
