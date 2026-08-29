using System.Runtime.Serialization;

namespace Resume.Application.DTO.Knowledge;

/// <summary>
/// Represents a loaded markdown document from the knowledge base.
/// </summary>
[DataContract]
public sealed class MarkdownDocumentDto
{
    /// <summary>
    /// Gets or sets the file name of the markdown document.
    /// </summary>
    [DataMember]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted title of the document.
    /// </summary>
    [DataMember]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full markdown content of the document.
    /// </summary>
    [DataMember]
    public string Content { get; set; } = string.Empty;
}
