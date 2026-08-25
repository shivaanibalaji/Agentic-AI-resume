# Learning Journal — Day 1: Building a RAG Pipeline from Scratch

## What Is This System?

This system lets a user ask a natural-language question like
*"What projects has Shivaani worked on?"* and receive a **grounded answer**
built exclusively from Shivaani's own Markdown resume files. Nothing is
invented. Every sentence in the answer can be traced back to a specific
source file, section, and chunk index.

The pipeline has five distinct intermediate stages:

```
Markdown on disk
  → 1. Load & parse
  → 2. Chunk by section
  → 3. Embed each chunk into a 1024-dim vector
  → 4. Store vectors in PostgreSQL (pgvector + HNSW)
  → 5. At query time: embed the question, find nearest vectors, build context, call LLM
```

Every stage is explained in detail below.

---

## Stage 1 — Markdown Loading

### What happens

`MarkdownDocumentLoader` walks the `Knowledge/` directory, reads every
`*.md` file, and returns a `MarkdownDocument` record per file:

```
MarkdownDocument(FileName, Title, Content)
```

### How the title is extracted

The loader reads lines top-to-bottom. The first line matching
`^#\s+(.+)` (a Markdown H1 heading) becomes the title.
If none is found, the filename without extension is used.
This means the title comes from the file itself — not from any
external metadata or database row.

### Why this matters for later stages

The title is stored in the `Documents` table and attached to every
chunk produced from that file. When the user receives sources in the
chat response, the `SourceDto` includes the filename and section —
all originating from this initial parse.

---

## Stage 2 — Chunking

### What is chunking?

LLMs have a fixed context window. You cannot stuff an entire
portfolio into a prompt. Chunking splits long documents into
overlapping pieces small enough to fit into a context window while
preserving enough surrounding text to remain meaningful on its own.

### The chunking algorithm in this codebase

`MarkdownChunker` uses a **two-pass** approach:

#### Pass 1: Split into sections by headings

The chunker scans every line and detects ATX headings
(`# ` through `###### `). When a heading is found, the accumulated
text above it becomes a named section:

```
Heading detected: "## Projects"
  → section name: "Projects"
  → body: everything since the last heading
```

Lines before any heading are grouped under `"(Introduction)"`.
After the final heading, the remaining text is flushed as the last
section. This means a Markdown file with 3 headings produces
4 sections (preamble + 3 heading sections).

**Section heading deduplication note:** if two sections in the same
file have the same heading text (e.g., two `##` subsections both
called "Details"), both will have the section name `"Details"`. The
chunk index still disambiguates them.

#### Pass 2: Split long sections into size-bounded chunks

Each section's content is checked against `ChunkSize` (default 1000
characters). If the section fits in one chunk, it stays whole.
Otherwise it is split into overlapping windows:

```
Section content (2500 chars):

[======Chunk 1======]
              [======Chunk 2======]
                        [======Chunk 3======]

Overlapping region = ChunkOverlap (default 150 chars)
```

The splitting prefers **paragraph boundaries**. Before cutting at the
hard limit, the algorithm looks for the last `\n\n` (double newline)
within the chunk window. If one exists in the upper half of the
window, the split happens there instead. This keeps paragraphs
intact, preserving semantic coherence.

The overlap guarantees that information near a split boundary
appears in two adjacent chunks, so a query embedding that falls
between them still retrieves relevant context.

### ChunkIndex assignment

A monotonically increasing `ChunkIndex` is assigned across the
entire document. If a file produces 8 chunks (from 4 sections),
the indices are 0 through 7. This index is stored in the database
and returned in `SearchResultDto` and `SourceDto` so the user can
locate exactly where the answer came from.

### What is stored per chunk

Each chunk becomes a `DocumentChunk` entity:

| Field | Purpose |
|-------|---------|
| `Id` | Primary key (Guid) |
| `DocumentId` | FK to parent `Document` |
| `Content` | The chunk text (up to ~1000 chars) |
| `Section` | Heading name (e.g., "Projects") |
| `ChunkIndex` | Sequential index within document |
| `Embedding` | 1024-dim pgvector `vector` column |
| `Metadata` | JSON string (character count, etc.) |
| `CreatedAt` | UTC timestamp |

---

## Stage 3 — Embedding Generation

### What is an embedding?

An embedding is a fixed-length numerical vector that captures the
**semantic meaning** of a piece of text. Two pieces of text about
the same topic will produce vectors that are close together in
1024-dimensional space, even if they use completely different words.

For example:
- "Built a RAG chatbot with .NET and pgvector"
- "Developed an AI-powered resume Q&A system"

These share semantic meaning, so their embeddings will be close —
much closer than "Shivaani's favorite color is blue."

### How embedding is done here

`OllamaEmbeddingService` calls the local Ollama API:

```
POST http://localhost:11434/api/embed
Body: { "model": "qwen3-embedding:0.6b", "input": "the chunk text" }
Response: { "embeddings": [[0.0231, -0.0045, ..., 0.0112]] }
```

The response contains a single inner array of 1024 floating-point
numbers. This array is the embedding vector.

### Why 1024 dimensions?

The model `qwen3-embedding:0.6b` is configured to produce 1024-
dimensional vectors. This is a design choice:

- **More dimensions** = richer semantic representation, but
  larger storage and slower search
- **Fewer dimensions** = faster search, but less semantic nuance

1024 is a common sweet spot for portfolio-scale document search.

### Dimension validation

The service **validates that the returned vector is exactly 1024
dimensions**. If the model returns a different count, an
`InvalidOperationException` is thrown with a clear message. The
vector is **never truncated or padded** — this would corrupt the
embedding and produce nonsensical search results.

### Where embeddings are generated

During ingestion (`IngestKnowledgeBaseCommandHandler`), every chunk
is embedded one at a time via `IEmbeddingService.GenerateEmbeddingAsync`.
The embedding is then wrapped in a `Pgvector.Vector` object and stored
in the `Embedding` column of `DocumentChunk`.

During query time (`SearchKnowledgeBaseQueryHandler` and
`AskPortfolioQuestionQueryHandler`), the user's question is embedded
using the same model. This produces a vector in the same 1024-
dimensional space, which can then be compared to stored chunk
vectors.

---

## Stage 4 — Storing Vectors in PostgreSQL (pgvector + HNSW)

### What is pgvector?

pgvector is a PostgreSQL extension that adds native support for
vector data types. It adds:

1. A `vector(n)` column type for storing fixed-dimension vectors
2. Distance operators: `<->` (L2), `<=>` (cosine), `<#>` (inner product)
3. Index types: HNSW and IVFFlat for fast approximate nearest-neighbor search

### How pgvector is enabled

In `ResumeDbContext.OnModelCreating`:

```csharp
modelBuilder.HasPostgresExtension("vector");
```

This causes the EF migration to emit:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

On Supabase, the pgvector extension (v0.8.2) was already available.
The migration simply enables it.

### How the column is configured

In `DocumentChunkConfiguration`:

```csharp
builder.Property(c => c.Embedding)
    .IsRequired()
    .HasColumnType("vector(1024)");
```

This tells EF Core to create the column as:

```sql
"Embedding" vector(1024) NOT NULL
```

The `(1024)` means the column only accepts vectors of exactly 1024
dimensions. Inserting a vector of any other length causes a
PostgreSQL error at the database level — a hard guarantee beyond
the C# validation.

### What is an HNSW index?

HNSW stands for **Hierarchical Navigable Small World**. It is an
approximate nearest-neighbor (ANN) algorithm that provides fast
vector similarity search without scanning every row.

Without an index, a cosine similarity search over 42 chunks would
scan all 42 rows — trivial at this scale. But at 100,000+ chunks,
a full scan would take seconds. HNSW reduces this to milliseconds
by pre-building a graph structure.

#### How HNSW works conceptually

HNSW builds a multi-layer graph where:

- **Layer 0** (bottom): contains ALL vectors, densely connected
- **Layer 1, 2, ...**: progressively fewer vectors, forming a
  "highway" for fast coarse navigation

At search time:

1. Start at the top layer (sparse graph)
2. Find the closest node in the current layer
3. Move down one layer, using the previous match as the entry point
4. Repeat until layer 0
5. In layer 0, perform a greedy search among densely connected neighbors
6. Return the top-K closest vectors found

This is analogous to finding a location on a map: first zoom to
the right country (high layers), then the right city (mid layers),
then the right street (layer 0).

#### HNSW configuration in EF Core

```csharp
builder.HasIndex(c => c.Embedding)
    .HasMethod("hnsw")
    .HasOperators("vector_cosine_ops");
```

This generates:

```sql
CREATE INDEX "IX_DocumentChunks_Embedding"
ON "DocumentChunks" USING hnsw ("Embedding" vector_cosine_ops);
```

- `hnsw`: the index algorithm
- `vector_cosine_ops`: tells pgvector to optimize for cosine distance

The default HNSW parameters (`m=16`, `ef_construction=64`) are
used. These control graph density and build quality.

### What is cosine distance?

Cosine distance measures the **angle** between two vectors,
ignoring their magnitudes. It answers: "are these two texts
pointing in the same semantic direction?"

The `<=>` operator in pgvector computes:

```
cosine_distance(a, b) = 1 - cosine_similarity(a, b)
                       = 1 - (a · b) / (|a| × |b|)
```

| Distance | Meaning |
|----------|---------|
| 0.0 | Identical direction (semantically identical) |
| 0.5 | Perpendicular (unrelated) |
| 1.0 | Opposite direction (semantically opposite) |

For portfolio search, typical relevant chunks return distances
of 0.2–0.5. Irrelevant chunks return 0.7–1.0.

The codebase converts distance to a **score** for the API response:

```csharp
Score = Math.Round(Math.Clamp(1.0 - CosineDistance, 0.0, 1.0), 4)
```

So a score of 0.7 means "70% similar."

### The vector search query

When a user asks a question, the system runs this raw SQL
(via `VectorSearchRepository`):

```sql
SELECT d."FileName"  AS "DocumentFileName",
       c."Section"   AS "Section",
       c."ChunkIndex" AS "ChunkIndex",
       c."Content"   AS "Content",
       c."Embedding" <=> @query AS "CosineDistance"
FROM "DocumentChunks" c
JOIN "Documents" d ON d."Id" = c."DocumentId"
ORDER BY c."Embedding" <=> @query
LIMIT @topK
```

Step by step:

1. `@query` is the 1024-dim embedding of the user's question,
   passed as a `Pgvector.Vector` parameter to Npgsql
2. `<=>` computes the cosine distance between the query vector
   and every stored chunk vector
3. `ORDER BY ... <=>` sorts by distance ascending (closest first)
4. `LIMIT @topK` returns only the top-K most relevant chunks
5. The HNSW index makes this operation approximately O(log n)
   instead of O(n)

**This is NOT calculated in C#.** The distance computation and
ranking happen entirely inside PostgreSQL, leveraging the HNSW
index for performance.

---

## Stage 5 — RAG Response Generation

### The full flow at query time

When `POST /api/chat` receives `{"message": "What projects has Shivaani worked on?"}`:

#### Step 5a: Embed the question

```
Ollama POST /api/embed
Input: "What projects has Shivaani worked on?"
Output: [0.0342, -0.0198, ..., 0.0087]   (1024 floats)
```

#### Step 5b: HNSW vector search

The question embedding is sent to PostgreSQL via the raw SQL query
above. The HNSW index finds the 5 nearest chunks in the embedding
space.

Example result:

```
Chunk 0: about.md     / Profile          / distance 0.31
Chunk 1: projects.md  / Overview         / distance 0.34
Chunk 2: about.md     / Career Goal      / distance 0.38
Chunk 3: experience.md/ Responsibilities  / distance 0.41
Chunk 4: skills.md    / Generative AI    / distance 0.45
```

#### Step 5c: Build the RAG context

The retrieved chunks are assembled into a numbered context block:

```
Portfolio context:

[1] Source: about.md | Section: Profile | Chunk: 0
My name is Shivaani Balaji. I am a software engineer...

[2] Source: projects.md | Section: Overview | Chunk: 0
The AI Resume and Portfolio RAG Chatbot allows users...

[3] Source: about.md | Section: Career Goal | Chunk: 4
My goal is to continue growing as a backend...

[4] Source: experience.md | Section: Primary Responsibilities | Chunk: 1
Built microservices using .NET and PostgreSQL...

[5] Source: skills.md | Section: Generative AI | Chunk: 7
Experience with RAG pipelines, Ollama, pgvector...
```

#### Step 5d: Call the LLM with grounding prompt

The system prompt is a strict grounding rule:

```
Answer the user's question using ONLY the supplied portfolio
context. Do not invent or assume personal information. If the
context does not contain enough information, say that the
information is not available in the portfolio.
```

The user prompt contains the numbered context + the original question.

Ollama receives this as a chat completion request:

```json
{
  "model": "qwen3:8b",
  "messages": [
    { "role": "system", "content": "Answer using ONLY..." },
    { "role": "user", "content": "Portfolio context:\n[1]...\n\nQuestion: ..." }
  ],
  "stream": false,
  "think": false,
  "options": { "temperature": 0.2 }
}
```

- `stream: false` — wait for the full response
- `think: false` — disables qwen3's internal reasoning chain
  (dramatically reduces response time from minutes to seconds)
- `temperature: 0.2` — low randomness, deterministic answers

#### Step 5e: Post-process the LLM response

The raw response may contain `<think>...</think>` blocks
(qwen3's reasoning trace). These are stripped via regex:

```csharp
regex: <think>.*?</think>  (singleline, case-insensitive)
```

If the stripped result is empty, the fallback message is:
*"The information is not available in the portfolio."*

#### Step 5f: Return structured response

```json
{
  "answer": "Shivaani has worked on an AI Resume and Portfolio RAG
    Chatbot, which is an AI-powered application that uses
    Retrieval-Augmented Generation...",
  "sources": [
    { "document": "about.md",      "section": "Profile",           "chunkIndex": 0 },
    { "document": "projects.md",   "section": "Overview",          "chunkIndex": 0 },
    { "document": "about.md",      "section": "Career Goal",       "chunkIndex": 4 },
    { "document": "experience.md", "section": "Responsibilities",  "chunkIndex": 1 },
    { "document": "skills.md",     "section": "Generative AI",     "chunkIndex": 7 }
  ]
}
```

---

## The Ingestion Pipeline (POST /api/knowledge/ingest)

The full ingestion flow, step by step:

```
1. Load all .md files from Knowledge/ directory
     ↓
2. For each file:
     a. Check if Document exists in DB (by FileName)
     b. If not: INSERT new Document row
     c. If exists but title changed: UPDATE title + UpdatedAt
     d. DELETE all existing DocumentChunk rows for this document
        (makes reruns idempotent)
     e. Split content into sections (by headings)
     f. Split long sections into 1000-char chunks with 150-char overlap
     g. For each chunk:
        - POST to Ollama /api/embed → get 1024-dim vector
        - Validate dimension == 1024
        - INSERT DocumentChunk row with embedding
     ↓
3. Return IngestionResultDto { TotalFiles=5, NewDocuments=5,
   UpdatedDocuments=0, TotalChunks=42 }
```

### Why reruns are safe

On a second ingest call:

- `Document` already exists → `newDocuments=0`
- Title unchanged → `updatedDocuments=0`
- Old chunks are deleted, new chunks are re-created
- Embeddings are regenerated from the same model
- Result is identical to a fresh ingest

This means you can add new `.md` files to `Knowledge/` and re-run
ingest. Existing documents are not duplicated; only new files
create new documents.

---

## The CQRS Pattern

Every user action goes through one of two paths:

| Path | Record | Handler | Return |
|------|--------|---------|--------|
| **Command** | `IngestKnowledgeBaseCommand` | `IngestKnowledgeBaseCommandHandler` | `IngestionResultDto` |
| **Query** | `SearchKnowledgeBaseQuery` | `SearchKnowledgeBaseQueryHandler` | `IReadOnlyList<SearchResultDto>` |
| **Query** | `AskPortfolioQuestionQuery` | `AskPortfolioQuestionQueryHandler` | `ChatResponseDto` |

MediatR dispatches these through a single pipeline. Controllers
never call handlers directly — they call `_mediator.Send(request)`.

### Why CQRS here

- **Write path (Ingest):** reads files, generates embeddings, writes
  to DB — heavy I/O, could take minutes
- **Read path (Search/Ask):** embeds a question, searches DB,
  calls LLM — reads-heavy with one LLM call
- Separating them allows independent optimization and testing

---

## The Repository Pattern

There are three repositories — none is generic:

| Repository | Responsibility |
|------------|---------------|
| `DocumentRepository` | Get/add/update `Document` rows |
| `DocumentChunkRepository` | Delete bulk chunks, bulk insert chunks |
| `VectorSearchRepository` | Raw SQL cosine search returning `KnowledgeChunkHit` |

Handlers **never touch `DbContext` directly**. They communicate
through repository interfaces. This means:

- You could swap EF Core for Dapper or raw Npgsql without changing
  any handler code
- You can unit test handlers by mocking repositories
- The raw SQL for vector search is isolated in one place

---

## The DI Wiring

### Application layer

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...));
```

Registers all `IRequestHandler<,>` implementations in the
Application assembly.

### Infrastructure layer

```csharp
services.AddDbContext<ResumeDbContext>(o => o.UseNpgsql(cs, npg => npg.UseVector()));
services.AddScoped<IDocumentRepository, DocumentRepository>();
services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
services.AddScoped<IVectorSearchRepository, VectorSearchRepository>();
services.Configure<KnowledgeBaseOptions>(configuration.GetSection("KnowledgeBase"));
services.AddSingleton<IMarkdownChunker, MarkdownChunker>();
services.AddScoped<IMarkdownDocumentLoader, MarkdownDocumentLoader>();
services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));
services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(...);
services.AddHttpClient<ILlmService, OllamaLlmService>(...);
```

Key details:

- `UseVector()` is the critical call from `Pgvector.EntityFrameworkCore`
  that registers the `Vector` CLR type mapping. Without it, EF Core
  cannot map `Vector` to the `vector(1024)` PostgreSQL column.
- Typed `HttpClient` instances (`IEmbeddingService`, `ILlmService`)
  are registered via `AddHttpClient<Interface, Implementation>`,
  which manages the lifecycle and timeout of each client.
- `IMarkdownChunker` is singleton (stateless, no DB dependency).
- Repositories and loaders are scoped (one per HTTP request).

---

## The Design-Time Factory

`ResumeDbContextDesignTimeFactory` exists solely for `dotnet ef`
migrations tooling. When you run `dotnet ef migrations add`, EF
needs to instantiate `ResumeDbContext` — but the real connection
string may not be set yet. The factory provides a fallback:

```csharp
var cs = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=resume_design_time;...";
```

This allows migrations to be generated and applied even when
the Supabase credentials are not in `appsettings.json`.

At runtime, the real connection string flows through DI, and
this factory is never called.

---

## Connection String Resolution

At application startup:

```csharp
var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? configuration.GetConnectionString("Supabase");
```

Priority:
1. `SUPABASE_CONNECTION_STRING` environment variable (used in
   production and during `dotnet ef database update`)
2. `ConnectionStrings:Supabase` in `appsettings.json` (for local
   development)
3. If neither is set → `InvalidOperationException` at startup

The connection string uses **keyword format**, not URI format:

```
Host=db.nkmstzpkudrqpgdenzep.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require
```

URI format (`postgresql://postgres:pass@host/db`) was found to
cause parsing issues with Npgsql 10 in certain configurations.
Keyword format is always reliable.

---

## Configuration Sections

```json
{
  "KnowledgeBase": {
    "Path": "../Knowledge",
    "ChunkSize": 1000,
    "ChunkOverlap": 150
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "qwen3-embedding:0.6b",
    "EmbeddingDimensions": 1024,
    "ChatModel": "qwen3:8b"
  }
}
```

- `Path` is relative to the API's `ContentRootPath` (the
  `Resume.Api/` folder). `Program.cs` normalizes it to an absolute
  path at startup so it works regardless of the shell's current
  directory.
- `ChunkSize` and `ChunkOverlap` are read once at startup via
  `IOptions<KnowledgeBaseOptions>`. Changing them requires a restart.

---

## How the Embedding Cosine Search Actually Works (End to End)

Here is what happens in PostgreSQL when a vector search query arrives:

```
1. Npgsql sends a parameterized query:
   SELECT ... ORDER BY "Embedding" <=> $1 LIMIT $2
   $1 = Vector([0.0342, -0.0198, ..., 0.0087])  (the question embedding)
   $2 = 5  (topK)

2. PostgreSQL receives the query and sees the <=> operator
   on the "Embedding" column of type vector(1024)

3. PostgreSQL checks for an index on "Embedding" and finds
   "IX_DocumentChunks_Embedding" (HNSW, vector_cosine_ops)

4. The HNSW index is consulted:
   a. Start at the highest layer of the HNSW graph
   b. Find the node closest to the query vector in that layer
   c. Descend to the next layer, using the previous match as
      the entry point
   d. Repeat until layer 0
   e. In layer 0, perform a greedy neighbor search among
      densely connected nodes
   f. Collect the 5 closest vectors found

5. For each of the 5 candidate vectors, PostgreSQL computes
   the exact cosine distance:
   distance = 1 - (query · chunk) / (|query| × |chunk|)

6. Results are sorted by distance (ascending = most similar first)

7. The query returns 5 rows with the chunk content, section,
   chunk index, filename, and cosine distance
```

The HNSW search is **approximate** — it may miss the true nearest
neighbor in rare cases — but for portfolio-scale data (42 chunks),
the approximation is effectively exact. The index becomes critical
at scales of 10,000+ chunks where a full scan would be too slow.

---

## What Was Tested and Verified

| Test | Method | Result |
|------|--------|--------|
| Build | `dotnet build` | 0 errors, 0 warnings |
| Migration generation | `dotnet ef migrations add InitialCreate` | Success — vector(1024), HNSW index, pgvector extension |
| Migration application | `dotnet ef database update` | Applied to Supabase successfully |
| Health check | `GET /api/health` | `{ status: "Healthy", database: "online" }` |
| Ingestion | `POST /api/knowledge/ingest` | 5 files, 5 documents, 42 chunks, 34 seconds |
| Ingestion rerun | `POST /api/knowledge/ingest` (second call) | 0 new, 0 updated — idempotent |
| Embedding dimension validation | Direct SQL query | All 42 embeddings = exactly 1024 dims |
| HNSW index existence | `pg_indexes` query | `IX_DocumentChunks_Embedding USING hnsw (vector_cosine_ops)` |
| pgvector extension | `pg_extension` query | `vector` v0.8.2 |
| RAG chat | `POST /api/chat` | Grounded answer + 5 source citations in ~100s |
| Ollama embedding API | Direct `POST /api/embed` | 1024-dim vector confirmed |
| Ollama chat API | Direct `POST /api/chat` | Valid answer from qwen3:8b |

---

## Commands to Reproduce Everything

```powershell
# 1. Set Supabase connection string (session-only)
$env:SUPABASE_CONNECTION_STRING = 'Host=db.nkmstzpkudrqpgdenzep.supabase.co;Port=5432;Database=postgres;Username=postgres;YOUR_PASSWORD;SSL Mode=Require'

# 2. Restore and build
dotnet restore
dotnet build

# 3. Apply migration to Supabase
dotnet ef database update --project Resume.Infrastructure --startup-project Resume.Api

# 4. Run the API
dotnet run --project Resume.Api --no-launch-profile --urls http://localhost:5080

# 5. Health check
Invoke-RestMethod http://localhost:5080/api/health

# 6. Ingest knowledge
Invoke-RestMethod -Uri http://localhost:5080/api/knowledge/ingest -Method Post

# 7. Chat
$body = '{"message":"What projects has Shivaani worked on?"}'
Invoke-RestMethod -Uri http://localhost:5080/api/chat -Method Post -Body $body -ContentType "application/json"
```

---

## Remaining Work

- **GPU acceleration** for qwen3:8b — reduces chat response from
  ~100s to ~2-5s
- **Chat history / sessions** — not implemented per spec
- **Authentication, rate limiting** — not implemented per spec
- **SearchKnowledgeBaseQuery has no API endpoint** — the handler
  exists but no controller route was specified for Day 1

---

*Generated from live testing against Supabase (pgvector 0.8.2) and
Ollama (qwen3-embedding:0.6b + qwen3:8b) on 2026-08-23.*
