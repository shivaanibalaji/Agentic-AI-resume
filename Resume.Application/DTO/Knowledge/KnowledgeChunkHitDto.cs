using System.Runtime.Serialization;

namespace Resume.Application.DTO.Knowledge;

/// <summary>
/// Represents a knowledge chunk returned as a vector search hit, including its
/// cosine distance from the query embedding.
/// </summary>
[DataContract]
public sealed class KnowledgeChunkHitDto
{
    /// <summary>
    /// Gets or sets the file name of the source document.
    /// </summary>
    [DataMember]
    public string DocumentFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heading of the section the chunk belongs to.
    /// </summary>
    [DataMember]
    public string Section { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zero-based index of the chunk within its document.
    /// </summary>
    [DataMember]
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the text content of the chunk.
    /// </summary>
    [DataMember]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cosine distance between the chunk embedding and the query embedding.
    /// </summary>
    [DataMember]
    public double CosineDistance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this chunk was marked as an overview, summary,
    /// or introduction during ingestion.
    /// </summary>
    [DataMember]
    public bool IsSummary { get; set; }
}
