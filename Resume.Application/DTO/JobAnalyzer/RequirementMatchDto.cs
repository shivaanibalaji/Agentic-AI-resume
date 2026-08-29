using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents the match result for a single job requirement against the portfolio knowledge base.
/// </summary>
[DataContract]
public sealed class RequirementMatchDto
{
    /// <summary>
    /// Gets or sets the text of the job requirement that was matched.
    /// </summary>
    [DataMember]
    public string Requirement { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the match status of the requirement ("Strong", "Partial", or "Weak").
    /// </summary>
    [DataMember]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the portfolio content retrieved as evidence for the requirement.
    /// </summary>
    [DataMember]
    public IReadOnlyList<PortfolioContentDto> MatchingContent { get; set; } = [];
}
