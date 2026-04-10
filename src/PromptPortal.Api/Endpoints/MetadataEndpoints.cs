using MediatR;
using PromptPortal.Application.Metadata.ListCategories;
using PromptPortal.Application.Metadata.ListTags;

namespace PromptPortal.Api.Endpoints;

public static class MetadataEndpoints
{
    public static WebApplication MapMetadataEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/metadata").WithTags("Metadata");

        group.MapGet("/tags", async (ISender sender) =>
        {
            var result = await sender.Send(new ListTagsQuery());
            return Results.Ok(result);
        })
        .WithName("ListTags")
        .WithSummary("List all distinct tags");

        group.MapGet("/categories", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCategoriesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCategories")
        .WithSummary("List all distinct categories");

        return app;
    }
}
