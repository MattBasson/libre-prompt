using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace PromptPortal.Infrastructure.Search;

public class OpenSearchIndexInitializer(
    IOpenSearchClient client,
    ILogger<OpenSearchIndexInitializer> logger
) : IHostedService
{
    private const string IndexName = "prompts-v1";

    public async Task StartAsync(CancellationToken ct)
    {
        const int maxRetries = 5;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var exists = await client.Indices.ExistsAsync(IndexName, c => c, ct);
                if (exists.Exists)
                {
                    logger.LogInformation("OpenSearch index '{IndexName}' already exists", IndexName);
                    return;
                }

                logger.LogInformation("Creating OpenSearch index '{IndexName}'...", IndexName);

                var createResponse = await client.Indices.CreateAsync(IndexName, c => c
                    .Map<OpenSearchSearchDocument>(m => m
                        .Properties(p => p
                            .Keyword(k => k.Name(n => n.PromptId))
                            .Keyword(k => k.Name(n => n.TenantId))
                            .Text(t => t.Name(n => n.Title).Fields(f => f.Keyword(k => k.Name("keyword"))))
                            .Text(t => t.Name(n => n.Summary))
                            .Text(t => t.Name(n => n.Content))
                            .Keyword(k => k.Name(n => n.Tags))
                            .Keyword(k => k.Name(n => n.Categories))
                            .Keyword(k => k.Name(n => n.Models))
                            .Keyword(k => k.Name(n => n.Status))
                            .Keyword(k => k.Name(n => n.CreatedBy))
                            .Date(d => d.Name(n => n.CreatedUtc))
                            .Date(d => d.Name(n => n.UpdatedUtc))
                        )
                    ),
                ct);

                if (createResponse.IsValid)
                {
                    logger.LogInformation("OpenSearch index '{IndexName}' created successfully", IndexName);
                    return;
                }

                logger.LogWarning("Failed to create OpenSearch index: {Error}", createResponse.ServerError?.Error?.Reason);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(ex, "OpenSearch not ready (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s...",
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }

        logger.LogError("Failed to create OpenSearch index '{IndexName}' after {MaxRetries} attempts", IndexName, maxRetries);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
