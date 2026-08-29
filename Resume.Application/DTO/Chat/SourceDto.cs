using System.Runtime.Serialization;

namespace Resume.Application.DTO.Chat;

/// <summary>
/// Represents a single source document referenced in a chat response.
/// </summary>
[DataContract]
public sealed class SourceDto
{
    /// <summary>
    /// Gets or sets the file name of the source document.
    /// </summary>
    [DataMember]
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heading of the source section.
    /// </summary>
    [DataMember]
    public string Section { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zero-based index of the source chunk.
    /// </summary>
    [DataMember]
    public int ChunkIndex { get; set; }
}
