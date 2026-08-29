using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resume.Application.Command.Create.AnalyzeJob;
using Resume.Application.DTO.JobAnalyzer;

namespace Resume.Api.Controllers;

/// <summary>
/// Handles job description analysis against the portfolio knowledge base.
/// </summary>
[ApiController]
[Route("api/job-analyzer")]
public class JobAnalyzerController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Analyzes a job description by matching its requirements against the portfolio knowledge base.
    /// </summary>
    /// <param name="request">The job analyzer request containing the job description.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The extracted information, per-requirement match results, and the overall match percentage.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JobAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobAnalysisResponseDto>> AnalyzeAsync(
        [FromBody] JobAnalyzerRequestDto request,
        CancellationToken cancellationToken)
    {
        JobAnalysisResponseDto response = await mediator.Send(
            new AnalyzeJobCommand(request.JobDescription),
            cancellationToken);

        return Ok(response);
    }
}
