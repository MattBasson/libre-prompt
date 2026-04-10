using MediatR;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Application.Metadata.ListTags;

public class ListTagsQueryHandler(
    IPromptSearchService searchService,
    ICurrentUserService currentUser
) : IRequestHandler<ListTagsQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(ListTagsQuery request, CancellationToken ct)
    {
        return await searchService.ListDistinctTagsAsync(currentUser.TenantId, ct);
    }
}
