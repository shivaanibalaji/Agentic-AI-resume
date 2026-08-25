using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resume.Application.Command.Create.IngestKnowledgeBase;
using Resume.Application.DTO.Knowledge;

namespace Resume.Api.Controllers;

[ApiController]
[Route("api/knowledge")]
public class KnowledgeController(IMediator mediator) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<ActionResult<IngestionResultDto>> Ingest(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IngestKnowledgeBaseCommand(), cancellationToken);

        return Ok(result);
    }
}
