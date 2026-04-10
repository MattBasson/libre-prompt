namespace PromptPortal.Domain.ValueObjects;

public class PromptVariable
{
    public string Name { get; set; } = string.Empty;
    public bool Required { get; set; }
}
