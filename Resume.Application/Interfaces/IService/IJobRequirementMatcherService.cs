using Resume.Application.DTO.JobAnalyzer;

namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Matches an individual job requirement against the portfolio knowledge base.
/// </summary>
public interface IJobRequirementMatcherService
{
    /// <summary>
    /// Generates an embedding for the requirement, searches the portfolio knowledge base,
    /// and classifies the match based on the retrieved content.
    /// </summary>
    /// <param name="requirement">The job requirement to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The match result including status and supporting portfolio content.</returns>
    Task<RequirementMatchDto> MatchAsync(string requirement, CancellationToken cancellationToken = default);
}
