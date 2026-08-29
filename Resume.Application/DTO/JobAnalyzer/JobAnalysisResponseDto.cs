using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents the result of analyzing a job description against the portfolio knowledge base.
/// </summary>
[DataContract]
public sealed class JobAnalysisResponseDto
{
    /// <summary>
    /// Gets or sets the overall match percentage of the portfolio to the job description (0 to 100).
    /// </summary>
    [DataMember]
    public int OverallPercentage { get; set; }

    /// <summary>
    /// Gets or sets the list of skills extracted from the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Skills { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of technologies extracted from the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Technologies { get; set; } = [];

    /// <summary>
    /// Gets or sets a summary of the experience level required by the job description.
    /// </summary>
    [DataMember]
    public string Experience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of individual requirements extracted from the job description.
    /// </summary>
    [DataMember]
    public IReadOnlyList<string> Requirements { get; set; } = [];

    /// <summary>
    /// Gets or sets the match result for each individual requirement.
    /// </summary>
    [DataMember]
    public IReadOnlyList<RequirementMatchDto> Matches { get; set; } = [];
}
