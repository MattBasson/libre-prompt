namespace PromptPortal.Application.Interfaces;

public interface IPromptSearchService
{
    Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListDistinctTagsAsync(string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListDistinctCategoriesAsync(string tenantId, CancellationToken ct = default);
}

public class SearchRequest
{
    public string? Query { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Models { get; set; }
    public string? Status { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchResult
{
    public List<SearchResultItem> Results { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public SearchFacets Facets { get; set; } = new();
}

public class SearchResultItem
{
    public string PromptId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public List<string> Models { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public class SearchFacets
{
    public List<FacetValue> Tags { get; set; } = [];
    public List<FacetValue> Categories { get; set; } = [];
    public List<FacetValue> Models { get; set; } = [];
}

public class FacetValue
{
    public string Value { get; set; } = string.Empty;
    public long Count { get; set; }
}
