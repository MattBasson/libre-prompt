using PromptPortal.Application.Interfaces;

namespace PromptPortal.Infrastructure.Embeddings;

public class NoOpEmbeddingService : IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        return Task.FromResult(Array.Empty<float>());
    }
}
