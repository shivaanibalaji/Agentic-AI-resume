using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents a job analyzer request containing the job description to analyze.
/// </summary>
[DataContract]
public sealed class JobAnalyzerRequestDto
{
    /// <summary>
    /// Gets or sets the job description to analyze.
    /// </summary>
    [DataMember]
    public string JobDescription { get; set; } = string.Empty;
}
