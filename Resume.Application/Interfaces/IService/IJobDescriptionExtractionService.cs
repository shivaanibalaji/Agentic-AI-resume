using Resume.Application.DTO.JobAnalyzer;

namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Extracts structured requirements from a job description using a language model.
/// </summary>
public interface IJobDescriptionExtractionService
{
    /// <summary>
    /// Extracts skills, requirements, technologies, and experience from the supplied job description.
    /// </summary>
    /// <param name="jobDescription">The job description to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The structured information extracted from the job description.</returns>
    Task<JobDescriptionExtractionDto> ExtractAsync(string jobDescription, CancellationToken cancellationToken = default);
}
