using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;
using PromptPortal.Domain.Enums;
using PromptPortal.Domain.ValueObjects;

namespace PromptPortal.Application.Prompts.UpdatePrompt;

public class UpdatePromptCommandHandler(
    IPromptRepository repository,
    IPromptSearchIndexer indexer,
    ICurrentUserService currentUser,
    IClock clock
) : IRequestHandler<UpdatePromptCommand, PromptDefinition>
{
    public async Task<PromptDefinition> Handle(UpdatePromptCommand request, CancellationToken ct)
    {
        var existing = await repository.GetByIdAsync(currentUser.TenantId, request.PromptId, ct)
            ?? throw new KeyNotFoundException($"Prompt '{request.PromptId}' not found.");

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.Summary = request.Summary ?? existing.Summary;
        existing.Tags = request.Tags;
        existing.Categories = request.Categories;
        existing.Models = request.Models;
        existing.PromptStyle = request.PromptStyle ?? existing.PromptStyle;
        existing.Language = request.Language ?? existing.Language;
        existing.Visibility = Enum.TryParse<PromptVisibility>(request.Visibility, true, out var vis)
            ? vis
            : existing.Visibility;
        existing.Variables = request.Variables?.Select(v => new PromptVariable { Name = v.Name, Required = v.Required }).ToList() ?? existing.Variables;
        existing.Examples = request.Examples?.Select(e => new PromptExample { Input = e.Input, OutputSummary = e.OutputSummary }).ToList() ?? existing.Examples;
        existing.Version++;
        existing.UpdatedUtc = clock.UtcNow;

        await repository.SaveAsync(existing, ct);
        await indexer.IndexAsync(existing, ct);

        return existing;
    }
}
