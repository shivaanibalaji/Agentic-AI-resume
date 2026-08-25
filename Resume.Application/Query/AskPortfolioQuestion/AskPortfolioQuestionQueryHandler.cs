using System.Text.RegularExpressions;
using MediatR;
using Resume.Application.DTO.Chat;
using Resume.Application.Interfaces;

namespace Resume.Application.Query.AskPortfolioQuestion;

public class AskPortfolioQuestionQueryHandler(
    IEmbeddingService embeddingService,
    IVectorSearchRepository vectorSearchRepository,
    ILlmService llmService)
    : IRequestHandler<AskPortfolioQuestionQuery, ChatResponseDto>
{
    private const int DefaultTopK = 5;

    private const string SystemPrompt =
        "Answer the user's question using ONLY the supplied portfolio context. " +
        "Do not invent or assume personal information. If the context does not contain enough information, " +
        "say that the information is not available in the portfolio.";

    private static readonly Regex ThinkingBlockRegex =
        new("<think>.*?</think>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public async Task<ChatResponseDto> Handle(AskPortfolioQuestionQuery request, CancellationToken cancellationToken)
    {
        var topK = request.TopK <= 0 ? DefaultTopK : request.TopK;

        var questionEmbedding = await embeddingService.GenerateEmbeddingAsync(request.Message, cancellationToken);
        var hits = await vectorSearchRepository.SearchAsync(questionEmbedding, topK, cancellationToken);

        if (hits.Count == 0)
        {
            return new ChatResponseDto(
                "The information is not available in the portfolio.",
                Array.Empty<SourceDto>());
        }

        var context = string.Join(
            "\n\n",
            hits.Select((hit, index) =>
                $"[{index + 1}] Source: {hit.DocumentFileName} | Section: {hit.Section} | Chunk: {hit.ChunkIndex}\n{hit.Content}"));

        var userPrompt = $"""
            Portfolio context:

            {context}

            Question: {request.Message}
            """;

        var rawAnswer = await llmService.GenerateAnswerAsync(SystemPrompt, userPrompt, cancellationToken);
        var answer = ThinkingBlockRegex.Replace(rawAnswer, string.Empty).Trim();

        if (answer.Length == 0)
        {
            answer = "The information is not available in the portfolio.";
        }

        var sources = hits
            .Select(hit => new SourceDto(hit.DocumentFileName, hit.Section, hit.ChunkIndex))
            .ToList();

        return new ChatResponseDto(answer, sources);
    }
}
