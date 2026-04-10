using MediatR;

namespace PromptPortal.Application.Metadata.ListCategories;

public record ListCategoriesQuery : IRequest<IReadOnlyList<string>>;
