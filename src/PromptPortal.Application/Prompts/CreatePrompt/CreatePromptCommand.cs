using MediatR;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.CreatePrompt;

public record CreatePromptCommand(
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

public record VariableInput(string Name, bool Required);
public record ExampleInput(string Input, string OutputSummary);
