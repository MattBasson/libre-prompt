# Prompt Portal — Implementation Plan & Spec

> **Purpose:** A Claude Code–ready plan for building a prompt library web portal.
> Feed this document to Claude Code as context when starting work on any phase.

---

## 1. Project Overview

**Prompt Portal** is a searchable web portal for storing, tagging, categorising, versioning, and discovering prompts used with Copilot, Claude, and other LLMs. The system uses a .NET + Angular stack with a local-first development approach orchestrated by .NET Aspire and Podman, designed for eventual AWS deployment.

### Core Value Proposition

- Store prompts as rich documents with flexible metadata (tags, categories, model compatibility, variables, examples).
- Discover prompts through hybrid search — keyword, semantic, and filtered — so users find the right prompt whether they remember its name, a tag, or just the intent.
- Version prompts over time and track lineage.

---

## 2. Technology Stack

| Layer | Technology | Notes |
|-------|-----------|-------|
| Frontend | Angular 19/20 (standalone app) | Served via Aspire `AddNpmApp` locally |
| Backend API | ASP.NET Core (.NET 10), Minimal APIs | Vertical slice / feature-folder structure |
| Orchestration | .NET Aspire 13 AppHost | Local dev orchestration with Podman |
| Container Runtime | Podman | Set `DOTNET_ASPIRE_CONTAINER_RUNTIME=podman` |
| Data Store | DynamoDB Local (container) | Source of truth for prompt documents |
| Search Engine | OpenSearch (container) | Searchable projection — keyword + future vector |
| Auth (local) | Fake dev auth | Fixed local tenant `local-dev`, no real identity yet |
| Auth (production) | Amazon Cognito + BFF pattern | Future phase |
| Embeddings | `IEmbeddingService` (no-op initially) | Amazon Bedrock later |
| Cloud Target | AWS (S3+CloudFront, App Runner, DynamoDB, OpenSearch Service) | Future phase |
| CI/CD | GitHub Actions | Future phase |

---

## 3. Solution Structure

```
prompt-portal/
├── src/
│   ├── PromptPortal.AppHost/           # Aspire orchestration
│   │   └── Program.cs
│   ├── PromptPortal.ServiceDefaults/   # Shared service config (health, telemetry)
│   ├── PromptPortal.Api/               # Minimal API endpoints, Swagger, health checks
│   │   ├── Endpoints/
│   │   │   ├── PromptEndpoints.cs
│   │   │   ├── SearchEndpoints.cs
│   │   │   └── MetadataEndpoints.cs
│   │   ├── Middleware/
│   │   │   └── DevAuthMiddleware.cs
│   │   └── Program.cs
│   ├── PromptPortal.Domain/            # Entities, value objects, enums
│   │   ├── Entities/
│   │   │   ├── PromptDefinition.cs
│   │   │   └── PromptVersion.cs
│   │   └── ValueObjects/
│   │       └── TagSet.cs
│   ├── PromptPortal.Application/       # Commands, queries, validators, interfaces
│   │   ├── Prompts/
│   │   │   ├── CreatePrompt/
│   │   │   ├── UpdatePrompt/
│   │   │   ├── GetPrompt/
│   │   │   ├── ListPrompts/
│   │   │   ├── ClonePrompt/
│   │   │   ├── ArchivePrompt/
│   │   │   └── SearchPrompts/
│   │   ├── Metadata/
│   │   │   ├── ListTags/
│   │   │   └── ListCategories/
│   │   └── Interfaces/
│   │       ├── IPromptRepository.cs
│   │       ├── IPromptSearchIndexer.cs
│   │       ├── IPromptSearchService.cs
│   │       ├── IEmbeddingService.cs
│   │       ├── ICurrentUserService.cs
│   │       ├── IClock.cs
│   │       └── IIdGenerator.cs
│   ├── PromptPortal.Infrastructure/    # DynamoDB, OpenSearch, dev auth implementations
│   │   ├── Persistence/
│   │   │   └── DynamoDbPromptRepository.cs
│   │   ├── Search/
│   │   │   ├── OpenSearchPromptIndexer.cs
│   │   │   └── OpenSearchPromptSearchService.cs
│   │   ├── Embeddings/
│   │   │   └── NoOpEmbeddingService.cs
│   │   └── Auth/
│   │       └── LocalDevUserService.cs
│   └── prompt-portal-web/              # Angular SPA
│       ├── src/
│       │   └── app/
│       │       ├── core/               # API client service, interceptors, config
│       │       ├── features/
│       │       │   └── prompts/        # list, edit, detail, search components
│       │       ├── shared/             # Reusable UI components (chips, filters)
│       │       └── models/             # TypeScript interfaces for prompt contracts
│       ├── angular.json
│       └── package.json
├── tests/
│   ├── PromptPortal.Api.Tests/
│   ├── PromptPortal.Application.Tests/
│   └── PromptPortal.IntegrationTests/
├── prompt-portal.slnx
└── README.md
```

---

## 4. Data Model

### 4.1 DynamoDB Table Design

**Table name:** `PromptLibrary`

| Key | Pattern | Example |
|-----|---------|---------|
| PK (Partition Key) | `TENANT#{tenantId}` | `TENANT#local-dev` |
| SK (Sort Key) | `PROMPT#{promptId}` | `PROMPT#prompt_01HZY` |

**Core attributes:**

```json
{
  "PK": "TENANT#local-dev",
  "SK": "PROMPT#prompt_01HZY",
  "EntityType": "Prompt",
  "PromptId": "prompt_01HZY",
  "Title": "Summarise architecture decision record",
  "Slug": "summarise-adr",
  "Content": "You are an enterprise architect...",
  "Summary": "Summarises ADRs into concise executive briefings.",
  "Tags": ["architecture", "adr", "summary"],
  "Categories": ["engineering", "documentation"],
  "Models": ["copilot", "gpt-4.1", "claude"],
  "PromptStyle": "instructional",
  "Language": "en-GB",
  "Visibility": "private",
  "Status": "active",
  "Version": 4,
  "SupersedesPromptId": "prompt_01HY...",
  "Variables": [
    { "Name": "audience", "Required": true },
    { "Name": "tone", "Required": false }
  ],
  "Examples": [
    { "Input": "ADR text...", "OutputSummary": "Short example result" }
  ],
  "CreatedBy": "user@company.com",
  "CreatedUtc": "2026-04-09T19:00:00Z",
  "UpdatedUtc": "2026-04-09T19:00:00Z"
}
```

**Future GSIs (add only when access patterns demand them):**

- `GSI1`: `PK = USER#{createdBy}`, `SK = UPDATED#{updatedUtc}` — for "my prompts" sorted by recency.
- `GSI2`: `PK = TAG#{tag}`, `SK = PROMPT#{promptId}` — only if DynamoDB-native tag browsing is needed outside OpenSearch.

### 4.2 OpenSearch Index

**Index name:** `prompts-v1`

| Field | OpenSearch Type | Purpose |
|-------|----------------|---------|
| `promptId` | `keyword` | Unique ID |
| `tenantId` | `keyword` | Tenant filter |
| `title` | `text` + `keyword` | Full-text search + exact match |
| `summary` | `text` | Full-text search |
| `content` | `text` | Full-text search |
| `tags` | `keyword` (array) | Filter / facet |
| `categories` | `keyword` (array) | Filter / facet |
| `models` | `keyword` (array) | Filter / facet |
| `status` | `keyword` | Filter |
| `createdBy` | `keyword` | Filter |
| `createdUtc` | `date` | Sort / filter |
| `updatedUtc` | `date` | Sort |
| `contentEmbedding` | `knn_vector` | **Future** — vector search |

---

## 5. API Contract

### 5.1 Prompt Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/prompts` | Create a new prompt |
| `PUT` | `/api/prompts/{id}` | Update an existing prompt (bumps version) |
| `GET` | `/api/prompts/{id}` | Get a single prompt by ID |
| `GET` | `/api/prompts?tag=x&category=y` | List prompts with optional filters |
| `POST` | `/api/prompts/search` | Hybrid search with query + filters |
| `POST` | `/api/prompts/{id}/clone` | Clone a prompt as a new draft |
| `POST` | `/api/prompts/{id}/archive` | Soft-archive a prompt |

### 5.2 Metadata Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/metadata/tags` | List all distinct tags |
| `GET` | `/api/metadata/categories` | List all distinct categories |

### 5.3 Health

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/health` | API + DynamoDB + OpenSearch readiness |

### 5.4 Request/Response Models

**Create/Update Prompt Request:**

```json
{
  "title": "Summarise ADR",
  "content": "You are an enterprise architect...",
  "summary": "Summarises an architecture decision record.",
  "tags": ["architecture", "adr"],
  "categories": ["engineering", "documentation"],
  "models": ["copilot", "claude"],
  "promptStyle": "instructional",
  "language": "en-GB",
  "visibility": "private",
  "variables": [
    { "name": "audience", "required": true }
  ],
  "examples": [
    { "input": "ADR text...", "outputSummary": "Short result" }
  ]
}
```

**Search Request:**

```json
{
  "query": "architecture decision summary",
  "tags": ["adr"],
  "categories": ["engineering"],
  "models": ["copilot"],
  "status": "active",
  "page": 1,
  "pageSize": 20
}
```

**Search Response:**

```json
{
  "results": [
    {
      "promptId": "prompt_01HZY",
      "title": "Summarise ADR",
      "summary": "Summarises an architecture decision record.",
      "tags": ["architecture", "adr"],
      "categories": ["engineering"],
      "models": ["copilot", "claude"],
      "status": "active",
      "score": 12.34,
      "updatedUtc": "2026-04-09T19:00:00Z"
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 20,
  "facets": {
    "tags": [{ "value": "architecture", "count": 15 }],
    "categories": [{ "value": "engineering", "count": 22 }],
    "models": [{ "value": "copilot", "count": 30 }]
  }
}
```

---

## 6. Core Interfaces

These are the abstractions the Application layer depends on. Infrastructure provides the implementations.

```csharp
public interface IPromptRepository
{
    Task<PromptDefinition> GetByIdAsync(string tenantId, string promptId, CancellationToken ct);
    Task<IReadOnlyList<PromptDefinition>> ListAsync(string tenantId, PromptFilter filter, CancellationToken ct);
    Task SaveAsync(PromptDefinition prompt, CancellationToken ct);
    Task DeleteAsync(string tenantId, string promptId, CancellationToken ct);
}

public interface IPromptSearchIndexer
{
    Task IndexAsync(PromptDefinition prompt, CancellationToken ct);
    Task RemoveAsync(string promptId, CancellationToken ct);
}

public interface IPromptSearchService
{
    Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken ct);
    Task<IReadOnlyList<string>> ListDistinctTagsAsync(string tenantId, CancellationToken ct);
    Task<IReadOnlyList<string>> ListDistinctCategoriesAsync(string tenantId, CancellationToken ct);
}

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct);
}

public interface ICurrentUserService
{
    string UserId { get; }
    string TenantId { get; }
    IReadOnlyList<string> Roles { get; }
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IIdGenerator
{
    string NewPromptId();
}
```

---

## 7. Aspire AppHost Configuration

The `PromptPortal.AppHost/Program.cs` should orchestrate:

1. **DynamoDB Local** — container on port 8000.
2. **OpenSearch** — container on port 9200 (with initial admin password env var for security bootstrap).
3. **OpenSearch Dashboards** — container on port 5601 (optional, for index inspection).
4. **ASP.NET Core API** — project reference, with DynamoDB and OpenSearch endpoints injected.
5. **Angular SPA** — via `AddNpmApp("web", "../prompt-portal-web")`.

**Podman note:** Set `DOTNET_ASPIRE_CONTAINER_RUNTIME=podman` before running the AppHost if both Docker and Podman are installed.

**API configuration injected by AppHost:**

| Setting | Value |
|---------|-------|
| `Persistence:Provider` | `DynamoDb` |
| `Persistence:DynamoDb:ServiceUrl` | `http://localhost:8000` |
| `Search:Provider` | `OpenSearch` |
| `Search:OpenSearch:Url` | `http://localhost:9200` |
| `Auth:Mode` | `LocalDev` |

---

## 8. Angular Frontend

### 8.1 UI Screens

| Route | Component | Description |
|-------|-----------|-------------|
| `/prompts` | PromptListComponent | Paginated list of prompts with tag/category chips |
| `/prompts/new` | PromptEditorComponent | Create a new prompt with metadata |
| `/prompts/:id` | PromptDetailComponent | View prompt with full metadata, version history |
| `/prompts/:id/edit` | PromptEditorComponent | Edit existing prompt |
| `/search` | SearchComponent | Free-text query + facet sidebar (tags, categories, models) |

### 8.2 Layout

- **Top:** Search bar (always visible).
- **Left sidebar:** Facet filters for tags, categories, models.
- **Centre:** Results list or prompt content.
- **Editor:** Markdown/plain-text body input, metadata chips for tags/categories/models, variable definitions, example input/output pairs.

### 8.3 Frontend Folder Structure

```
src/app/
├── core/
│   ├── services/
│   │   ├── prompt-api.service.ts       # Typed HTTP client for the API
│   │   └── search-api.service.ts
│   ├── interceptors/
│   │   └── api-base-url.interceptor.ts
│   └── config/
│       └── environment.ts
├── features/
│   └── prompts/
│       ├── prompt-list/
│       ├── prompt-detail/
│       ├── prompt-editor/
│       └── prompt-search/
├── shared/
│   ├── components/
│   │   ├── tag-chips/
│   │   ├── category-chips/
│   │   └── model-chips/
│   └── pipes/
└── models/
    ├── prompt.model.ts
    └── search.model.ts
```

---

## 9. Security Model (Local Dev)

For local development, skip real identity. The API reads a fixed dev user from configuration:

- **User:** `dev-user@local`
- **Tenant:** `local-dev`
- **Roles:** `PromptReader`, `PromptEditor`, `PromptAdmin`

The `DevAuthMiddleware` injects this user into `ICurrentUserService` on every request. Every prompt is scoped to the `local-dev` tenant.

**Production auth (future):** Amazon Cognito with BFF pattern in ASP.NET Core, using app roles `PromptReader`, `PromptEditor`, `PromptAdmin` and row-level filtering by `tenantId` + `visibility`.

---

## 10. Implementation Phases

### Phase 1 — Foundation (Sprint 1–2)

> **Goal:** CRUD + keyword search + tags/categories working end-to-end locally.

| # | Task | Details |
|---|------|---------|
| 1.1 | Create .NET 10 Aspire solution | `slnx`, AppHost, ServiceDefaults, Api, Domain, Application, Infrastructure projects |
| 1.2 | Wire Angular into AppHost | `AddNpmApp`, configure proxy for API calls |
| 1.3 | Add DynamoDB Local container to AppHost | Port 8000, configure Aspire container resource |
| 1.4 | Add OpenSearch container to AppHost | Port 9200, set `OPENSEARCH_INITIAL_ADMIN_PASSWORD` |
| 1.5 | Implement domain model | `PromptDefinition`, `PromptVersion`, `TagSet`, enums |
| 1.6 | Implement `IPromptRepository` with DynamoDB | CRUD operations against DynamoDB Local |
| 1.7 | Implement Prompt CRUD endpoints | `POST/PUT/GET /api/prompts`, clone, archive |
| 1.8 | Implement OpenSearch indexing | Index prompt on create/update via `IPromptSearchIndexer` |
| 1.9 | Implement keyword search endpoint | `POST /api/prompts/search` with text + metadata filters |
| 1.10 | Implement metadata endpoints | `GET /api/metadata/tags`, `GET /api/metadata/categories` |
| 1.11 | Add Swagger / OpenAPI | Auto-generated API docs |
| 1.12 | Add health check endpoint | API, DynamoDB, OpenSearch readiness |
| 1.13 | Build Angular prompt list page | Fetch and display prompts |
| 1.14 | Build Angular prompt editor page | Create/edit with tag/category/model chips |
| 1.15 | Build Angular prompt detail page | View prompt with metadata |
| 1.16 | Build Angular search page | Free-text + facet filters |
| 1.17 | Add dev auth middleware | Fixed local user injection |

### Phase 2 — Versioning & Polish (Sprint 3)

| # | Task | Details |
|---|------|---------|
| 2.1 | Prompt versioning | Auto-increment version on update, store history |
| 2.2 | Version history UI | Show version timeline on prompt detail |
| 2.3 | Prompt preview with variables | Render prompt with variable substitution |
| 2.4 | Favourites | Mark and filter favourite prompts |
| 2.5 | Import/Export | JSON and Markdown import/export |
| 2.6 | Unit + integration tests | Repository, search, endpoint tests |

### Phase 3 — Semantic Search (Sprint 4)

| # | Task | Details |
|---|------|---------|
| 3.1 | Implement `IEmbeddingService` with Bedrock | Replace no-op with Amazon Bedrock Titan embeddings |
| 3.2 | Add `knn_vector` field to OpenSearch index | Map `contentEmbedding` field |
| 3.3 | Generate embeddings on create/update | Embed title + content + summary |
| 3.4 | Implement hybrid search | Combine BM25 text + knn vector results |
| 3.5 | UI toggle for search mode | Keyword / Semantic / Hybrid selector |

### Phase 4 — Production Readiness (Sprint 5–6)

| # | Task | Details |
|---|------|---------|
| 4.1 | Amazon Cognito integration | Real identity with BFF pattern |
| 4.2 | RBAC enforcement | Role-based access on all endpoints |
| 4.3 | Multi-tenant isolation | Enforce `tenantId` scoping in queries |
| 4.4 | AWS deployment setup | S3+CloudFront, App Runner, DynamoDB, OpenSearch Service |
| 4.5 | GitHub Actions CI/CD | PR build, dev deploy, release promotion |
| 4.6 | Infrastructure as Code | Terraform or CDK for AWS resources |
| 4.7 | Observability | Application Insights / CloudWatch, structured logging |
| 4.8 | Audit trail | Log prompt changes to a DynamoDB audit container |

---

## 11. Key Design Decisions

1. **DynamoDB as source of truth, OpenSearch as projection.** DynamoDB handles writes and operational lookups. OpenSearch handles discovery. Don't try to make the database do search or the search engine do writes.

2. **Interfaces first.** All infrastructure is hidden behind interfaces (`IPromptRepository`, `IPromptSearchService`, `IEmbeddingService`). This keeps the move from local-only to AWS-hosted as a hosting concern, not a rewrite.

3. **No-op embedding initially.** Ship CRUD and keyword search before introducing vector complexity. The `IEmbeddingService` abstraction means Bedrock can be wired in later without changing the API or search contracts.

4. **Aspire for local orchestration only.** Aspire manages the local dev graph (API, Angular, DynamoDB, OpenSearch). It does not dictate how you deploy to AWS.

5. **Single-table DynamoDB, but keep it simple.** Start with a straightforward `TENANT#/PROMPT#` key design. Add GSIs only when a real access pattern demands them.

6. **BFF auth pattern for production.** Keep OAuth complexity out of the Angular app. ASP.NET Core handles OIDC, cookies, and token management as the authenticated boundary.

---

## 12. Local Development Quick Start

```bash
# Prerequisites
# - .NET 10 SDK
# - Node.js 20+
# - Podman installed and running

# Set Podman as the container runtime for Aspire
export DOTNET_ASPIRE_CONTAINER_RUNTIME=podman

# Clone and restore
git clone <repo-url> prompt-portal
cd prompt-portal

# Restore .NET dependencies
dotnet restore prompt-portal.slnx

# Install Angular dependencies
cd src/prompt-portal-web && npm install && cd ../..

# Run everything via Aspire
dotnet run --project src/PromptPortal.AppHost

# Aspire dashboard: https://localhost:18888
# API + Swagger: https://localhost:5001/swagger
# Angular app:   http://localhost:4200
# DynamoDB Local: http://localhost:8000
# OpenSearch:     http://localhost:9200
# OpenSearch Dashboards: http://localhost:5601
```

---

## 13. Claude Code Usage Notes

When working with Claude Code on this project, reference this document and specify which phase/task you're working on. Example prompts:

- *"Read PROMPT_PORTAL_PLAN.md. Create the .NET 10 Aspire solution structure as defined in Section 3. Use slnx format. Set up the AppHost per Section 7."*
- *"Implement task 1.6 — the DynamoDbPromptRepository class. Follow the DynamoDB table design in Section 4.1 and implement the IPromptRepository interface from Section 6."*
- *"Build the Angular prompt editor component (task 1.14). Use the request model from Section 5.4 and the folder structure from Section 8.3."*
- *"Implement the OpenSearch search service (task 1.9). Use the index mapping from Section 4.2 and return the search response format from Section 5.4."*

---

*Last updated: 2026-04-09*
