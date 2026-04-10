var builder = DistributedApplication.CreateBuilder(args);

// DynamoDB Local — runs as a local container, no AWS account needed
var dynamoDb = builder.AddContainer("dynamodb", "amazon/dynamodb-local", "latest")
    .WithEndpoint(port: 8000, targetPort: 8000, name: "dynamodb", scheme: "http");

// OpenSearch — runs as a local container with security disabled for dev
var openSearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "2.19.1")
    .WithEndpoint(port: 9200, targetPort: 9200, name: "http", scheme: "http")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
    .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m");

// OpenSearch Dashboards — optional dev tool for inspecting indices
builder.AddContainer("opensearch-dashboards", "opensearchproject/opensearch-dashboards", "2.19.1")
    .WithEndpoint(port: 5601, targetPort: 5601, name: "http", scheme: "http")
    .WithEnvironment("OPENSEARCH_HOSTS", "[\"http://host.containers.internal:9200\"]")
    .WithEnvironment("DISABLE_SECURITY_DASHBOARDS_PLUGIN", "true");

// API project with references to local infrastructure
var api = builder.AddProject<Projects.PromptPortal_Api>("api")
    .WithEnvironment("Persistence__DynamoDb__ServiceUrl", dynamoDb.GetEndpoint("dynamodb"))
    .WithEnvironment("Search__OpenSearch__Url", openSearch.GetEndpoint("http"))
    .WithEnvironment("Auth__Mode", "LocalDev")
    .WaitFor(dynamoDb)
    .WaitFor(openSearch);

// Angular SPA
builder.AddNpmApp("portal-web", "../prompt-portal-web", "start")
    .WithReference(api)
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
