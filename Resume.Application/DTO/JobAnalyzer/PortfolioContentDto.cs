using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents a piece of portfolio content retrieved as evidence for a job requirement.
/// </summary>
[DataContract]
public sealed class PortfolioContentDto
{
    /// <summary>
    /// Gets or sets the file name of the source portfolio document.
    /// </summary>
    [DataMember]
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heading of the section the content belongs to.
    /// </summary>
    [DataMember]
    public string Section { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zero-based index of the content chunk within its document.
    /// </summary>
    [DataMember]
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the text content of the matching portfolio chunk.
    /// </summary>
    [DataMember]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relevance score of the content (0 to 1).
    /// </summary>
    [DataMember]
    public double Score { get; set; }
}
