using System.Text.RegularExpressions;
using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;
using PromptPortal.Domain.Enums;
using PromptPortal.Domain.ValueObjects;

namespace PromptPortal.Application.Prompts.CreatePrompt;

public partial class CreatePromptCommandHandler(
    IPromptRepository repository,
    IPromptSearchIndexer indexer,
    ICurrentUserService currentUser,
    IClock clock,
    IIdGenerator idGenerator
) : IRequestHandler<CreatePromptCommand, PromptDefinition>
{
    public async Task<PromptDefinition> Handle(CreatePromptCommand request, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var promptId = idGenerator.NewPromptId();

        var prompt = new PromptDefinition
        {
            PromptId = promptId,
            TenantId = currentUser.TenantId,
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Content = request.Content,
            Summary = request.Summary ?? string.Empty,
            Tags = request.Tags,
            Categories = request.Categories,
            Models = request.Models,
            PromptStyle = request.PromptStyle ?? "instructional",
            Language = request.Language ?? "en-GB",
            Visibility = Enum.TryParse<PromptVisibility>(request.Visibility, true, out var vis) ? vis : PromptVisibility.Private,
            Status = PromptStatus.Active,
            Version = 1,
            Variables = request.Variables?.Select(v => new PromptVariable { Name = v.Name, Required = v.Required }).ToList() ?? [],
            Examples = request.Examples?.Select(e => new PromptExample { Input = e.Input, OutputSummary = e.OutputSummary }).ToList() ?? [],
            CreatedBy = currentUser.UserId,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        await repository.SaveAsync(prompt, ct);
        await indexer.IndexAsync(prompt, ct);

        return prompt;
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = SlugRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
