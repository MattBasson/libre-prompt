using MediatR;

namespace PromptPortal.Application.Prompts.ArchivePrompt;

public record ArchivePromptCommand(string PromptId) : IRequest;
