using PromptPortal.Application.Interfaces;

namespace PromptPortal.Infrastructure.Services;

public class GuidIdGenerator : IIdGenerator
{
    public string NewPromptId() => $"prompt_{Guid.NewGuid():N}";
}
