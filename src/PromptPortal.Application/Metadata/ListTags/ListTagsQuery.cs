using MediatR;

namespace PromptPortal.Application.Metadata.ListTags;

public record ListTagsQuery : IRequest<IReadOnlyList<string>>;
