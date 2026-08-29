using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Resume.Application.DTO.Chat;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;

namespace Resume.Application.Query.AskPortfolioQuestion;

/// <summary>
/// Query that asks a question about the portfolio and streams an answer with sources.
/// </summary>
/// <param name="Message">The question to ask.</param>
/// <param name="TopK">The maximum number of context chunks to retrieve.</param>
public sealed record AskPortfolioQuestionQuery(string Message, int TopK = 5) : IRequest<IAsyncEnumerable<ChatStreamChunkDto>>;

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
/// Handler that answers a portfolio question using retrieval-augmented generation,
/// streaming the answer chunks as they become available.
/// </summary>
public sealed class AskPortfolioQuestionQueryHandler(
    IEmbeddingService embeddingService,
    IVectorSearchRepository vectorSearchRepository,
    IRetrievalRankingService retrievalRankingService,
    ILlmService llmService,
    IOptions<KnowledgeBaseOptions> options)
    : IRequestHandler<AskPortfolioQuestionQuery, IAsyncEnumerable<ChatStreamChunkDto>>
{
    private const int DefaultTopK = 5;

    private const string SystemPrompt =
        "Answer the user's question using ONLY the supplied portfolio context. " +
        "Do not invent or assume personal information. If the context does not contain enough information, " +
        "say that the information is not available in the portfolio.";

    private const string NoContextAnswer = "The information is not available in the portfolio.";

    private static readonly Regex ThinkingBlockRegex =
        new(" thinking.*? response", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Answers the supplied question by retrieving relevant context and streaming the generated response.
    /// </summary>
    /// <param name="request">The portfolio question query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream of answer chunks ending with a terminal chunk that carries the sources.</returns>
    public Task<IAsyncEnumerable<ChatStreamChunkDto>> Handle(AskPortfolioQuestionQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(StreamAnswerAsync(request, cancellationToken));
    }

    private async IAsyncEnumerable<ChatStreamChunkDto> StreamAnswerAsync(
        AskPortfolioQuestionQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int topK = request.TopK <= 0 ? DefaultTopK : request.TopK;
        int candidatePoolSize = Math.Max(topK, options.Value.CandidatePoolSize);

        float[] questionEmbedding = await embeddingService.GenerateEmbeddingAsync(request.Message, cancellationToken);
        IReadOnlyList<KnowledgeChunkHitDto> candidates = await vectorSearchRepository.SearchCandidatesAsync(questionEmbedding, candidatePoolSize, cancellationToken);
        IReadOnlyList<SearchResultDto> results = retrievalRankingService.ReRank(candidates, request.Message, topK);

        List<SourceDto> sources = results
            .Select(result => new SourceDto
            {
                Document = result.Document,
                Section = result.Section,
                ChunkIndex = result.ChunkIndex
            })
            .ToList();

        if (results.Count == 0)
        {
            yield return new ChatStreamChunkDto
            {
                AnswerPart = NoContextAnswer,
                Sources = sources,
                Done = true
            };

            yield break;
        }

        string context = string.Join(
            "\n\n",
            results.Select((result, index) =>
                $"[{index + 1}] Source: {result.Document} | Section: {result.Section} | Chunk: {result.ChunkIndex}\n{result.Content}"));

        string userPrompt = $"""
            Portfolio context:

            {context}

            Question: {request.Message}
            """;

        StringBuilder answerBuilder = new();

        await foreach (string chunk in llmService.GenerateAnswerStreamAsync(SystemPrompt, userPrompt, cancellationToken))
        {
            yield return new ChatStreamChunkDto { AnswerPart = chunk };
            answerBuilder.Append(chunk);
        }

        string answer = ThinkingBlockRegex.Replace(answerBuilder.ToString(), string.Empty).Trim();

        if (answer.Length == 0)
        {
            yield return new ChatStreamChunkDto { AnswerPart = NoContextAnswer };
        }

        yield return new ChatStreamChunkDto
        {
            Sources = sources,
            Done = true
        };
    }
}
