using MediatR;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.GetPrompt;

public record GetPromptQuery(string PromptId) : IRequest<PromptDefinition?>;
