using FluentValidation;
using MediatR;
using Resume.Application.DTO.JobAnalyzer;
using Resume.Application.Interfaces.IService;

namespace Resume.Application.Command.Create.AnalyzeJob;

/// <summary>
/// Command that analyzes a job description against the portfolio knowledge base.
/// </summary>
/// <param name="JobDescription">The job description to analyze.</param>
public sealed record AnalyzeJobCommand(string JobDescription) : IRequest<JobAnalysisResponseDto>;

/// <summary>
/// Validates that a job analyzer command contains a non-empty job description.
/// </summary>
public sealed class AnalyzeJobCommandValidator : AbstractValidator<AnalyzeJobCommand>
{
    /// <summary>
    /// Defines the validation rules for <see cref="AnalyzeJobCommand"/>.
    /// </summary>
    public AnalyzeJobCommandValidator()
    {
        RuleFor(command => command.JobDescription)
            .NotEmpty().WithMessage("A non-empty job description is required.");
    }
}

/// <summary>
/// Handler that analyzes a job description by extracting requirements, matching each
/// requirement against the portfolio knowledge base, and computing an overall score.
/// </summary>
public sealed class AnalyzeJobCommandHandler(
    IJobDescriptionExtractionService extractionService,
    IJobRequirementMatcherService requirementMatcher)
    : IRequestHandler<AnalyzeJobCommand, JobAnalysisResponseDto>
{
    private const double StrongMatchPoints = 1.0;
    private const double PartialMatchPoints = 0.5;
    private const double WeakMatchPoints = 0.0;

    /// <summary>
    /// Analyzes the supplied job description and produces a deterministic match result.
    /// </summary>
    /// <param name="request">The job analyzer command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response containing the overall score and per-requirement match results.</returns>
    public async Task<JobAnalysisResponseDto> Handle(AnalyzeJobCommand request, CancellationToken cancellationToken)
    {
        JobDescriptionExtractionDto extraction = await extractionService.ExtractAsync(
            request.JobDescription,
            cancellationToken);

        List<RequirementMatchDto> matches = new List<RequirementMatchDto>(extraction.Requirements.Count);

        foreach (string requirement in extraction.Requirements)
        {
            RequirementMatchDto match = await requirementMatcher.MatchAsync(requirement, cancellationToken);
            matches.Add(match);
        }

        int requirementsCount = matches.Count;

        if (requirementsCount == 0)
        {
            return new JobAnalysisResponseDto
            {
                OverallPercentage = 0,
                Skills = extraction.Skills,
                Technologies = extraction.Technologies,
                Experience = extraction.Experience,
                Requirements = extraction.Requirements,
                Matches = matches
            };
        }

        double totalPoints = matches.Sum(match => match.Status switch
        {
            nameof(MatchStatus.Strong) => StrongMatchPoints,
            nameof(MatchStatus.Partial) => PartialMatchPoints,
            _ => WeakMatchPoints
        });

        int overallPercentage = (int)Math.Round((totalPoints / requirementsCount) * 100.0);

        return new JobAnalysisResponseDto
        {
            OverallPercentage = overallPercentage,
            Skills = extraction.Skills,
            Technologies = extraction.Technologies,
            Experience = extraction.Experience,
            Requirements = extraction.Requirements,
            Matches = matches
        };
    }
}
