using Microsoft.AspNetCore.Http;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Infrastructure.Auth;

public class LocalDevUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "dev-user@local";

    public string TenantId =>
        httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value ?? "local-dev";

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User?.FindAll("role")?.Select(c => c.Value).ToList()
        ?? ["PromptReader", "PromptEditor", "PromptAdmin"];
}
