using MediatR;
using PromptPortal.Application.Prompts.SearchPrompts;

namespace PromptPortal.Api.Endpoints;

public static class SearchEndpoints
{
    public static WebApplication MapSearchEndpoints(this WebApplication app)
    {
        app.MapPost("/api/prompts/search", async (SearchPromptsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithTags("Search")
        .WithName("SearchPrompts")
        .WithSummary("Hybrid search with query + filters");

        return app;
    }
}
