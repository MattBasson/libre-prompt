using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;
using PromptPortal.Domain.Enums;
using PromptPortal.Domain.ValueObjects;

namespace PromptPortal.Application.Prompts.ClonePrompt;

public class ClonePromptCommandHandler(
    IPromptRepository repository,
    IPromptSearchIndexer indexer,
    ICurrentUserService currentUser,
    IClock clock,
    IIdGenerator idGenerator
) : IRequestHandler<ClonePromptCommand, PromptDefinition>
{
    public async Task<PromptDefinition> Handle(ClonePromptCommand request, CancellationToken ct)
    {
        var source = await repository.GetByIdAsync(currentUser.TenantId, request.SourcePromptId, ct)
            ?? throw new KeyNotFoundException($"Prompt '{request.SourcePromptId}' not found.");

        var now = clock.UtcNow;
        var newId = idGenerator.NewPromptId();

        var clone = new PromptDefinition
        {
            PromptId = newId,
            TenantId = currentUser.TenantId,
            Title = request.NewTitle ?? $"{source.Title} (Copy)",
            Slug = $"{source.Slug}-copy-{newId[..8].ToLowerInvariant()}",
            Content = source.Content,
            Summary = source.Summary,
            Tags = [.. source.Tags],
            Categories = [.. source.Categories],
            Models = [.. source.Models],
            PromptStyle = source.PromptStyle,
            Language = source.Language,
            Visibility = PromptVisibility.Private,
            Status = PromptStatus.Active,
            Version = 1,
            SupersedesPromptId = source.PromptId,
            Variables = source.Variables.Select(v => new PromptVariable { Name = v.Name, Required = v.Required }).ToList(),
            Examples = source.Examples.Select(e => new PromptExample { Input = e.Input, OutputSummary = e.OutputSummary }).ToList(),
            CreatedBy = currentUser.UserId,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        await repository.SaveAsync(clone, ct);
        await indexer.IndexAsync(clone, ct);

        return clone;
    }
}
