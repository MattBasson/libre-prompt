namespace PromptPortal.Application.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string TenantId { get; }
    IReadOnlyList<string> Roles { get; }
}
