using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents the structured information extracted from a job description.
/// </summary>
[DataContract]
public sealed class JobDescriptionExtractionDto
{
    /// <summary>
    /// Gets or sets the list of skills mentioned in the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Skills { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of individual requirements extracted from the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Requirements { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of technologies mentioned in the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Technologies { get; set; } = [];

    /// <summary>
    /// Gets or sets a summary of the experience level required by the job description.
    /// </summary>
    [DataMember]
    public string Experience { get; set; } = string.Empty;
}
