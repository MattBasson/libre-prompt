using MediatR;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Application.Prompts.SearchPrompts;

public record SearchPromptsQuery(
    string? Query,
    List<string>? Tags,
    List<string>? Categories,
    List<string>? Models,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchResult>;
