using Pgvector;

namespace Resume.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public Vector Embedding { get; set; } = new(Array.Empty<float>());

    public string Metadata { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public Document Document { get; set; } = null!;
}
