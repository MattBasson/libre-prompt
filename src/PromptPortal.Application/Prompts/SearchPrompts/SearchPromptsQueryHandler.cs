using MediatR;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Application.Prompts.SearchPrompts;

public class SearchPromptsQueryHandler(
    IPromptSearchService searchService,
    ICurrentUserService currentUser
) : IRequestHandler<SearchPromptsQuery, SearchResult>
{
    public async Task<SearchResult> Handle(SearchPromptsQuery request, CancellationToken ct)
    {
        var searchRequest = new SearchRequest
        {
            Query = request.Query,
            Tags = request.Tags,
            Categories = request.Categories,
            Models = request.Models,
            Status = request.Status,
            TenantId = currentUser.TenantId,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return await searchService.SearchAsync(searchRequest, ct);
    }
}
