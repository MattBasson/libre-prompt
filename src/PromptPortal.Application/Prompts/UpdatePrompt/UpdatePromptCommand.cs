using MediatR;
using PromptPortal.Application.Prompts.CreatePrompt;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.UpdatePrompt;

public record UpdatePromptCommand(
    string PromptId,
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
) : IRequest<PromptDefinition>;
