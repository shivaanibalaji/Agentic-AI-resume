using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resume.Application.Command.Create.IngestKnowledgeBase;
using Resume.Application.DTO.Knowledge;

namespace Resume.Api.Controllers;

/// <summary>
/// Handles knowledge base operations.
/// </summary>
[ApiController]
[Route("api/knowledge")]
public class KnowledgeController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Ingests the markdown documents into the knowledge base.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the ingestion operation.</returns>
    [HttpPost("ingest")]
    [ProducesResponseType(typeof(IngestionResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestionResultDto>> IngestAsync(CancellationToken cancellationToken)
    {
        IngestionResultDto result = await mediator.Send(new IngestKnowledgeBaseCommand(), cancellationToken);

        return Ok(result);
    }
}
