using Pgvector;

namespace Resume.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this chunk represents an overview, summary,
    /// or introduction of its document. Computed once during ingestion from the section
    /// heading and used generically by retrieval ranking.
    /// </summary>
    public bool IsSummary { get; set; }

    public Vector Embedding { get; set; } = new(Array.Empty<float>());

    public string Metadata { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public Document Document { get; set; } = null!;
}
