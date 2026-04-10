using MediatR;
using PromptPortal.Application.Prompts.ArchivePrompt;
using PromptPortal.Application.Prompts.ClonePrompt;
using PromptPortal.Application.Prompts.CreatePrompt;
using PromptPortal.Application.Prompts.GetPrompt;
using PromptPortal.Application.Prompts.ListPrompts;
using PromptPortal.Application.Prompts.UpdatePrompt;

namespace PromptPortal.Api.Endpoints;

public static class PromptEndpoints
{
    public static WebApplication MapPromptEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/prompts").WithTags("Prompts");

        group.MapPost("/", async (CreatePromptCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/prompts/{result.PromptId}", result);
        })
        .WithName("CreatePrompt")
        .WithSummary("Create a new prompt");

        group.MapPut("/{id}", async (string id, UpdatePromptRequest request, ISender sender) =>
        {
            var command = new UpdatePromptCommand(
                id,
                request.Title,
                request.Content,
                request.Summary,
                request.Tags,
                request.Categories,
                request.Models,
                request.PromptStyle,
                request.Language,
                request.Visibility,
                request.Variables,
                request.Examples);

            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdatePrompt")
        .WithSummary("Update an existing prompt (bumps version)");

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetPromptQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetPrompt")
        .WithSummary("Get a single prompt by ID");

        group.MapGet("/", async (string? tag, string? category, ISender sender) =>
        {
            var result = await sender.Send(new ListPromptsQuery(tag, category));
            return Results.Ok(result);
        })
        .WithName("ListPrompts")
        .WithSummary("List prompts with optional filters");

        group.MapPost("/{id}/clone", async (string id, CloneRequest? request, ISender sender) =>
        {
            var result = await sender.Send(new ClonePromptCommand(id, request?.NewTitle));
            return Results.Created($"/api/prompts/{result.PromptId}", result);
        })
        .WithName("ClonePrompt")
        .WithSummary("Clone a prompt as a new draft");

        group.MapPost("/{id}/archive", async (string id, ISender sender) =>
        {
            await sender.Send(new ArchivePromptCommand(id));
            return Results.NoContent();
        })
        .WithName("ArchivePrompt")
        .WithSummary("Soft-archive a prompt");

        return app;
    }
}

public record UpdatePromptRequest(
    string Title,
    string Content,
    string? Summary,
    List<string> Tags,
    List<string> Categories,
    List<string> Models,
    string? PromptStyle,
    string? Language,
    string? Visibility,
    List<VariableInput>? Variables,
    List<ExampleInput>? Examples
);

public record CloneRequest(string? NewTitle);
