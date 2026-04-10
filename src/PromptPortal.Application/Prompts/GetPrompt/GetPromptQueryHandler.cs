using MediatR;
using PromptPortal.Application.Interfaces;
using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Prompts.GetPrompt;

public class GetPromptQueryHandler(
    IPromptRepository repository,
    ICurrentUserService currentUser
) : IRequestHandler<GetPromptQuery, PromptDefinition?>
{
    public async Task<PromptDefinition?> Handle(GetPromptQuery request, CancellationToken ct)
    {
        return await repository.GetByIdAsync(currentUser.TenantId, request.PromptId, ct);
    }
}
