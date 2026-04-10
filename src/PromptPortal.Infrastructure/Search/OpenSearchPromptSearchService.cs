using OpenSearch.Client;
using PromptPortal.Application.Interfaces;

namespace PromptPortal.Infrastructure.Search;

public class OpenSearchPromptSearchService(IOpenSearchClient client) : IPromptSearchService
{
    private const string IndexName = "prompts-v1";

    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken ct = default)
    {
        var response = await client.SearchAsync<OpenSearchSearchDocument>(s =>
        {
            s.Index(IndexName)
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize);

            s.Query(q => q.Bool(b =>
            {
                var must = new List<Func<QueryContainerDescriptor<OpenSearchSearchDocument>, QueryContainer>>();
                var filter = new List<Func<QueryContainerDescriptor<OpenSearchSearchDocument>, QueryContainer>>();

                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    must.Add(m => m.MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(p => p.Title, boost: 3)
                            .Field(p => p.Summary, boost: 2)
                            .Field(p => p.Content))
                        .Query(request.Query)
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(Fuzziness.Auto)));
                }

                filter.Add(f => f.Term(t => t.Field(p => p.TenantId).Value(request.TenantId)));

                if (request.Tags is { Count: > 0 })
                    filter.Add(f => f.Terms(t => t.Field(p => p.Tags).Terms(request.Tags)));

                if (request.Categories is { Count: > 0 })
                    filter.Add(f => f.Terms(t => t.Field(p => p.Categories).Terms(request.Categories)));

                if (request.Models is { Count: > 0 })
                    filter.Add(f => f.Terms(t => t.Field(p => p.Models).Terms(request.Models)));

                if (!string.IsNullOrWhiteSpace(request.Status))
                    filter.Add(f => f.Term(t => t.Field(p => p.Status).Value(request.Status)));

                b.Must(must.ToArray());
                b.Filter(filter.ToArray());

                return b;
            }));

            s.Aggregations(a => a
                .Terms("tags", t => t.Field(f => f.Tags).Size(50))
                .Terms("categories", t => t.Field(f => f.Categories).Size(50))
                .Terms("models", t => t.Field(f => f.Models).Size(50)));

            s.Sort(so => so.Descending(p => p.UpdatedUtc));

            return s;
        }, ct);

        var results = response.Documents.Select(d => new SearchResultItem
        {
            PromptId = d.PromptId,
            Title = d.Title,
            Summary = d.Summary,
            Tags = d.Tags,
            Categories = d.Categories,
            Models = d.Models,
            Status = d.Status,
            Score = 0,
            UpdatedUtc = d.UpdatedUtc
        }).ToList();

        if (response.Hits != null)
        {
            for (int i = 0; i < response.Hits.Count && i < results.Count; i++)
            {
                results[i].Score = response.Hits.ElementAt(i).Score ?? 0;
            }
        }

        return new SearchResult
        {
            Results = results,
            Total = (int)(response.Total),
            Page = request.Page,
            PageSize = request.PageSize,
            Facets = new SearchFacets
            {
                Tags = ExtractFacets(response.Aggregations, "tags"),
                Categories = ExtractFacets(response.Aggregations, "categories"),
                Models = ExtractFacets(response.Aggregations, "models")
            }
        };
    }

    public async Task<IReadOnlyList<string>> ListDistinctTagsAsync(string tenantId, CancellationToken ct = default)
    {
        var response = await client.SearchAsync<OpenSearchSearchDocument>(s => s
            .Index(IndexName)
            .Size(0)
            .Query(q => q.Term(t => t.Field(f => f.TenantId).Value(tenantId)))
            .Aggregations(a => a.Terms("tags", t => t.Field(f => f.Tags).Size(500))),
        ct);

        return ExtractFacets(response.Aggregations, "tags").Select(f => f.Value).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<string>> ListDistinctCategoriesAsync(string tenantId, CancellationToken ct = default)
    {
        var response = await client.SearchAsync<OpenSearchSearchDocument>(s => s
            .Index(IndexName)
            .Size(0)
            .Query(q => q.Term(t => t.Field(f => f.TenantId).Value(tenantId)))
            .Aggregations(a => a.Terms("categories", t => t.Field(f => f.Categories).Size(500))),
        ct);

        return ExtractFacets(response.Aggregations, "categories").Select(f => f.Value).ToList().AsReadOnly();
    }

    private static List<FacetValue> ExtractFacets(AggregateDictionary? aggregations, string name)
    {
        if (aggregations is null)
            return [];

        var terms = aggregations.Terms(name);
        if (terms is null)
            return [];

        return terms.Buckets.Select(b => new FacetValue
        {
            Value = b.Key,
            Count = b.DocCount ?? 0
        }).ToList();
    }
}
