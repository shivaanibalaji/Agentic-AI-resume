using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resume.Application.DTO.Chat;
using Resume.Application.Query.AskPortfolioQuestion;

namespace Resume.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "A non-empty 'message' is required." });
        }

        var response = await mediator.Send(new AskPortfolioQuestionQuery(request.Message), cancellationToken);

        return Ok(response);
    }
}
