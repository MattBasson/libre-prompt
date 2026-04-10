using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Interfaces;

public interface IPromptSearchIndexer
{
    Task IndexAsync(PromptDefinition prompt, CancellationToken ct = default);
    Task RemoveAsync(string promptId, CancellationToken ct = default);
}
