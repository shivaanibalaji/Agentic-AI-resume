using System.Runtime.Serialization;

namespace Resume.Application.DTO.JobAnalyzer;

/// <summary>
/// Represents the strength of a match between a job requirement and the portfolio knowledge base.
/// </summary>
[DataContract]
public enum MatchStatus
{
    /// <summary>
    /// The portfolio clearly demonstrates that the requirement is supported.
    /// </summary>
    [EnumMember]
    Strong,

    /// <summary>
    /// The portfolio shows related or partially matching experience.
    /// </summary>
    [EnumMember]
    Partial,

    /// <summary>
    /// There is little or no relevant evidence in the portfolio.
    /// </summary>
    [EnumMember]
    Weak
}
