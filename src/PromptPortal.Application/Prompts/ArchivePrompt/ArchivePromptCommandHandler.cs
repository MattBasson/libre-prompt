using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Enums;

namespace PromptPortal.Application.Prompts.ArchivePrompt;

public class ArchivePromptCommandHandler(
    IPromptRepository repository,
    IPromptSearchIndexer indexer,
    ICurrentUserService currentUser,
    IClock clock
) : IRequestHandler<ArchivePromptCommand>
{
    public async Task Handle(ArchivePromptCommand request, CancellationToken ct)
    {
        var prompt = await repository.GetByIdAsync(currentUser.TenantId, request.PromptId, ct)
            ?? throw new KeyNotFoundException($"Prompt '{request.PromptId}' not found.");

        prompt.Status = PromptStatus.Archived;
        prompt.UpdatedUtc = clock.UtcNow;

        await repository.SaveAsync(prompt, ct);
        await indexer.IndexAsync(prompt, ct);
    }
}
