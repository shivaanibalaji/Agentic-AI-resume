using System.Runtime.Serialization;

namespace Resume.Application.DTO.Knowledge;

/// <summary>
/// Represents the result of a knowledge base ingestion operation.
/// </summary>
[DataContract]
public sealed class IngestionResultDto
{
    /// <summary>
    /// Gets or sets the total number of markdown files processed.
    /// </summary>
    [DataMember]
    public int TotalFiles { get; set; }

    /// <summary>
    /// Gets or sets the number of newly created documents.
    /// </summary>
    [DataMember]
    public int NewDocuments { get; set; }

    /// <summary>
    /// Gets or sets the number of updated documents.
    /// </summary>
    [DataMember]
    public int UpdatedDocuments { get; set; }

    /// <summary>
    /// Gets or sets the total number of chunks stored.
    /// </summary>
    [DataMember]
    public int TotalChunks { get; set; }
}
