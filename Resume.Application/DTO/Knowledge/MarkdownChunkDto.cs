using System.Runtime.Serialization;

namespace Resume.Application.DTO.Knowledge;

/// <summary>
/// Represents a single chunk extracted from a markdown document.
/// </summary>
[DataContract]
public sealed class MarkdownChunkDto
{
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
}
