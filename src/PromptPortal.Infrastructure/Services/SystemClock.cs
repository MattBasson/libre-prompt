using PromptPortal.Application.Interfaces;

namespace PromptPortal.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
