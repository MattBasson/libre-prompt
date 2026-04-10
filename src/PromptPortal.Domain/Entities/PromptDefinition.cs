using PromptPortal.Domain.Enums;
using PromptPortal.Domain.ValueObjects;

namespace PromptPortal.Domain.Entities;

public class PromptDefinition
{
    public string PromptId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public List<string> Models { get; set; } = [];
    public string PromptStyle { get; set; } = "instructional";
    public string Language { get; set; } = "en-GB";
    public PromptVisibility Visibility { get; set; } = PromptVisibility.Private;
    public PromptStatus Status { get; set; } = PromptStatus.Active;
    public int Version { get; set; } = 1;
    public string? SupersedesPromptId { get; set; }
    public List<PromptVariable> Variables { get; set; } = [];
    public List<PromptExample> Examples { get; set; } = [];
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
