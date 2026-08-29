using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions StreamJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    /// <summary>
    /// Answers a question about the portfolio using retrieval-augmented generation.
    /// The answer is streamed back as server-sent events, chunk by chunk.
    /// </summary>
    /// <param name="request">The chat request containing the user's message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task ChatAsync(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<ChatStreamChunkDto> chunks = await mediator.Send(
            new AskPortfolioQuestionQuery(request.Message),
            cancellationToken);

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        try
        {
            await foreach (ChatStreamChunkDto chunk in chunks.WithCancellation(cancellationToken))
            {
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(chunk, StreamJsonOptions)}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                if (chunk.Done)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // The request was cancelled or the client disconnected; stop streaming.
        }
        catch (Exception)
        {
            if (Response.HasStarted)
            {
                await WriteErrorEventAsync();
            }
            else
            {
                throw;
            }
        }
    }

    private async Task WriteErrorEventAsync()
    {
        try
        {
            const string errorEvent =
                "data: {\"error\":\"An unexpected error occurred while generating the response.\",\"done\":true}\n\n";

            await Response.WriteAsync(errorEvent, CancellationToken.None);
            await Response.Body.FlushAsync(CancellationToken.None);
        }
        catch
        {
            // The client is gone; there is nothing more to write.
        }
    }
}