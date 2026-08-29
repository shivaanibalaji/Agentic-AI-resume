using System.Text.RegularExpressions;
using FluentValidation;
using MediatR;
using Resume.Application.DTO.Chat;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;

namespace Resume.Application.Query.AskPortfolioQuestion;

/// <summary>
/// Query that asks a question about the portfolio and returns an answer with sources.
/// </summary>
/// <param name="Message">The question to ask.</param>
/// <param name="TopK">The maximum number of context chunks to retrieve.</param>
public sealed record AskPortfolioQuestionQuery(string Message, int TopK = 5) : IRequest<ChatResponseDto>;

/// <summary>
/// Validates that a portfolio question contains a non-empty message.
/// </summary>
public sealed class AskPortfolioQuestionQueryValidator : AbstractValidator<AskPortfolioQuestionQuery>
{
    /// <summary>
    /// Defines the validation rules for <see cref="AskPortfolioQuestionQuery"/>.
    /// </summary>
    public AskPortfolioQuestionQueryValidator()
    {
        RuleFor(query => query.Message)
            .NotEmpty().WithMessage("A non-empty message is required.");
    }
}

/// <summary>
/// Handler that answers a portfolio question using retrieval-augmented generation.
/// </summary>
public sealed class AskPortfolioQuestionQueryHandler(
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
        new(" thinking.*? response", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Answers the supplied question by retrieving relevant context and generating a response.
    /// </summary>
    /// <param name="request">The portfolio question query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A chat response containing the answer and its sources.</returns>
    public async Task<ChatResponseDto> Handle(AskPortfolioQuestionQuery request, CancellationToken cancellationToken)
    {
        int topK = request.TopK <= 0 ? DefaultTopK : request.TopK;

        float[] questionEmbedding = await embeddingService.GenerateEmbeddingAsync(request.Message, cancellationToken);
        IReadOnlyList<KnowledgeChunkHitDto> hits = await vectorSearchRepository.SearchAsync(questionEmbedding, topK, cancellationToken);

        if (hits.Count == 0)
        {
            return new ChatResponseDto
            {
                Answer = "The information is not available in the portfolio.",
                Sources = Array.Empty<SourceDto>()
            };
        }

        string context = string.Join(
            "\n\n",
            hits.Select((hit, index) =>
                $"[{index + 1}] Source: {hit.DocumentFileName} | Section: {hit.Section} | Chunk: {hit.ChunkIndex}\n{hit.Content}"));

        string userPrompt = $"""
            Portfolio context:

            {context}

            Question: {request.Message}
            """;

        string rawAnswer = await llmService.GenerateAnswerAsync(SystemPrompt, userPrompt, cancellationToken);
        string answer = ThinkingBlockRegex.Replace(rawAnswer, string.Empty).Trim();

        if (answer.Length == 0)
        {
            answer = "The information is not available in the portfolio.";
        }

        List<SourceDto> sources = hits
            .Select(hit => new SourceDto
            {
                Document = hit.DocumentFileName,
                Section = hit.Section,
                ChunkIndex = hit.ChunkIndex
            })
            .ToList();

        return new ChatResponseDto
        {
            Answer = answer,
            Sources = sources
        };
    }
}
