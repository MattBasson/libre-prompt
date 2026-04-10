using Amazon.DynamoDBv2.Model;
using PromptPortal.Domain.Entities;
using PromptPortal.Domain.Enums;
using PromptPortal.Domain.ValueObjects;

namespace PromptPortal.Infrastructure.Persistence;

public static class PromptMapper
{
    public static Dictionary<string, AttributeValue> ToDynamoDb(PromptDefinition prompt)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new($"TENANT#{prompt.TenantId}"),
            ["SK"] = new($"PROMPT#{prompt.PromptId}"),
            ["EntityType"] = new("Prompt"),
            ["PromptId"] = new(prompt.PromptId),
            ["TenantId"] = new(prompt.TenantId),
            ["Title"] = new(prompt.Title),
            ["Slug"] = new(prompt.Slug),
            ["Content"] = new(prompt.Content),
            ["Summary"] = new(prompt.Summary),
            ["PromptStyle"] = new(prompt.PromptStyle),
            ["Language"] = new(prompt.Language),
            ["Visibility"] = new(prompt.Visibility.ToString()),
            ["Status"] = new(prompt.Status.ToString()),
            ["Version"] = new() { N = prompt.Version.ToString() },
            ["CreatedBy"] = new(prompt.CreatedBy),
            ["CreatedUtc"] = new(prompt.CreatedUtc.ToString("O")),
            ["UpdatedUtc"] = new(prompt.UpdatedUtc.ToString("O"))
        };

        if (prompt.Tags.Count > 0)
            item["Tags"] = new() { L = prompt.Tags.Select(t => new AttributeValue(t)).ToList() };

        if (prompt.Categories.Count > 0)
            item["Categories"] = new() { L = prompt.Categories.Select(c => new AttributeValue(c)).ToList() };

        if (prompt.Models.Count > 0)
            item["Models"] = new() { L = prompt.Models.Select(m => new AttributeValue(m)).ToList() };

        if (prompt.SupersedesPromptId is not null)
            item["SupersedesPromptId"] = new(prompt.SupersedesPromptId);

        if (prompt.Variables.Count > 0)
        {
            item["Variables"] = new()
            {
                L = prompt.Variables.Select(v => new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Name"] = new(v.Name),
                        ["Required"] = new() { BOOL = v.Required }
                    }
                }).ToList()
            };
        }

        if (prompt.Examples.Count > 0)
        {
            item["Examples"] = new()
            {
                L = prompt.Examples.Select(e => new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Input"] = new(e.Input),
                        ["OutputSummary"] = new(e.OutputSummary)
                    }
                }).ToList()
            };
        }

        return item;
    }

    public static PromptDefinition FromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new PromptDefinition
        {
            PromptId = item.GetValueOrDefault("PromptId")?.S ?? string.Empty,
            TenantId = item.GetValueOrDefault("TenantId")?.S ?? string.Empty,
            Title = item.GetValueOrDefault("Title")?.S ?? string.Empty,
            Slug = item.GetValueOrDefault("Slug")?.S ?? string.Empty,
            Content = item.GetValueOrDefault("Content")?.S ?? string.Empty,
            Summary = item.GetValueOrDefault("Summary")?.S ?? string.Empty,
            Tags = item.GetValueOrDefault("Tags")?.L?.Select(a => a.S).ToList() ?? [],
            Categories = item.GetValueOrDefault("Categories")?.L?.Select(a => a.S).ToList() ?? [],
            Models = item.GetValueOrDefault("Models")?.L?.Select(a => a.S).ToList() ?? [],
            PromptStyle = item.GetValueOrDefault("PromptStyle")?.S ?? "instructional",
            Language = item.GetValueOrDefault("Language")?.S ?? "en-GB",
            Visibility = Enum.TryParse<PromptVisibility>(item.GetValueOrDefault("Visibility")?.S, true, out var vis) ? vis : PromptVisibility.Private,
            Status = Enum.TryParse<PromptStatus>(item.GetValueOrDefault("Status")?.S, true, out var status) ? status : PromptStatus.Active,
            Version = int.TryParse(item.GetValueOrDefault("Version")?.N, out var ver) ? ver : 1,
            SupersedesPromptId = item.GetValueOrDefault("SupersedesPromptId")?.S,
            Variables = item.GetValueOrDefault("Variables")?.L?.Select(a => new PromptVariable
            {
                Name = a.M.GetValueOrDefault("Name")?.S ?? string.Empty,
                Required = a.M.GetValueOrDefault("Required")?.BOOL ?? false
            }).ToList() ?? [],
            Examples = item.GetValueOrDefault("Examples")?.L?.Select(a => new PromptExample
            {
                Input = a.M.GetValueOrDefault("Input")?.S ?? string.Empty,
                OutputSummary = a.M.GetValueOrDefault("OutputSummary")?.S ?? string.Empty
            }).ToList() ?? [],
            CreatedBy = item.GetValueOrDefault("CreatedBy")?.S ?? string.Empty,
            CreatedUtc = DateTimeOffset.TryParse(item.GetValueOrDefault("CreatedUtc")?.S, out var created) ? created : DateTimeOffset.MinValue,
            UpdatedUtc = DateTimeOffset.TryParse(item.GetValueOrDefault("UpdatedUtc")?.S, out var updated) ? updated : DateTimeOffset.MinValue
        };
    }
}
