using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using OpenSearch.Net;
using PromptPortal.Application.Interfaces;
using PromptPortal.Infrastructure.Auth;
using PromptPortal.Infrastructure.Embeddings;
using PromptPortal.Infrastructure.Health;
using PromptPortal.Infrastructure.Persistence;
using PromptPortal.Infrastructure.Search;
using PromptPortal.Infrastructure.Services;

namespace PromptPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DynamoDB Local
        var dynamoDbServiceUrl = configuration["Persistence:DynamoDb:ServiceUrl"] ?? "http://localhost:8000";
        services.AddSingleton<IAmazonDynamoDB>(_ =>
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = dynamoDbServiceUrl
            };
            return new AmazonDynamoDBClient("local", "local", config);
        });
        services.AddHostedService<DynamoDbTableInitializer>();
        services.AddScoped<IPromptRepository, DynamoDbPromptRepository>();

        // OpenSearch
        var openSearchUrl = configuration["Search:OpenSearch:Url"] ?? "http://localhost:9200";
        services.AddSingleton<IOpenSearchClient>(_ =>
        {
            var pool = new SingleNodeConnectionPool(new Uri(openSearchUrl));
            var settings = new ConnectionSettings(pool)
                .DefaultIndex("prompts-v1")
                .DefaultMappingFor<OpenSearchSearchDocument>(m => m.IndexName("prompts-v1"))
                .EnableDebugMode()
                .ServerCertificateValidationCallback((_, _, _, _) => true);
            return new OpenSearchClient(settings);
        });
        services.AddHostedService<OpenSearchIndexInitializer>();
        services.AddScoped<IPromptSearchIndexer, OpenSearchPromptIndexer>();
        services.AddScoped<IPromptSearchService, OpenSearchPromptSearchService>();

        // Auth
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, LocalDevUserService>();

        // Utilities
        services.AddSingleton<IEmbeddingService, NoOpEmbeddingService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();

        // Health checks
        services.AddHealthChecks()
            .AddCheck<DynamoDbHealthCheck>("dynamodb")
            .AddCheck<OpenSearchHealthCheck>("opensearch");

        return services;
    }
}
