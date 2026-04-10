using MediatR;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Application.Metadata.ListCategories;

public class ListCategoriesQueryHandler(
    IPromptSearchService searchService,
    ICurrentUserService currentUser
) : IRequestHandler<ListCategoriesQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        return await searchService.ListDistinctCategoriesAsync(currentUser.TenantId, ct);
    }
}
