using PromptPortal.Domain.Entities;

namespace PromptPortal.Application.Interfaces;

public interface IPromptRepository
{
    Task<PromptDefinition?> GetByIdAsync(string tenantId, string promptId, CancellationToken ct = default);
    Task<IReadOnlyList<PromptDefinition>> ListAsync(string tenantId, PromptFilter? filter = null, CancellationToken ct = default);
    Task SaveAsync(PromptDefinition prompt, CancellationToken ct = default);
    Task DeleteAsync(string tenantId, string promptId, CancellationToken ct = default);
}

public class PromptFilter
{
    public string? Tag { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
}
