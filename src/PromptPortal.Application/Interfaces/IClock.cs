namespace PromptPortal.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
