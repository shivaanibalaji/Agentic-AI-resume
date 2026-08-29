using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resume.Application.DTO.Chat;
using Resume.Application.Query.AskPortfolioQuestion;

namespace Resume.Api.Controllers;

/// <summary>
/// Handles chat requests against the portfolio knowledge base.
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Answers a question about the portfolio using retrieval-augmented generation.
    /// </summary>
    /// <param name="request">The chat request containing the user's message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated answer and its supporting sources.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponseDto>> ChatAsync(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        ChatResponseDto response = await mediator.Send(
            new AskPortfolioQuestionQuery(request.Message),
            cancellationToken);

        return Ok(response);
    }
}
