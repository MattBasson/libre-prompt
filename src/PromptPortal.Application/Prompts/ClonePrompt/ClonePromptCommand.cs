using MediatR;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.ClonePrompt;

public record ClonePromptCommand(string SourcePromptId, string? NewTitle = null) : IRequest<PromptDefinition>;
