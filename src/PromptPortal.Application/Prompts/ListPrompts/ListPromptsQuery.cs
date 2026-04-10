using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.ListPrompts;

public record ListPromptsQuery(string? Tag = null, string? Category = null) : IRequest<IReadOnlyList<PromptDefinition>>;
