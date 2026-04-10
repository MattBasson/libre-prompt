using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.ListPrompts;

public class ListPromptsQueryHandler(
    IPromptRepository repository,
    ICurrentUserService currentUser
) : IRequestHandler<ListPromptsQuery, IReadOnlyList<PromptDefinition>>
{
    public async Task<IReadOnlyList<PromptDefinition>> Handle(ListPromptsQuery request, CancellationToken ct)
    {
        var filter = new PromptFilter
        {
            Tag = request.Tag,
            Category = request.Category
        };

        return await repository.ListAsync(currentUser.TenantId, filter, ct);
    }
}
