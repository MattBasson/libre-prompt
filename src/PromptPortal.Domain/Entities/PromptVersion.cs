namespace PromptPortal.Domain.Entities;

public class PromptVersion
{
    public int VersionNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ChangeNote { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}
