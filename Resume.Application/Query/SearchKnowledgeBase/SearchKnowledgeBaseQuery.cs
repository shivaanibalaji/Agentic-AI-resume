using FluentValidation;
using MediatR;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;

namespace Resume.Application.Query.SearchKnowledgeBase;

/// <summary>
/// Query that searches the knowledge base for chunks relevant to a question.
/// </summary>
/// <param name="Question">The question used to search the knowledge base.</param>
/// <param name="TopK">The maximum number of results to return.</param>
public sealed record SearchKnowledgeBaseQuery(string Question, int TopK = 5) : IRequest<IReadOnlyList<SearchResultDto>>;

/// <summary>
/// Validates that a knowledge base search contains a non-empty question.
/// </summary>
public sealed class SearchKnowledgeBaseQueryValidator : AbstractValidator<SearchKnowledgeBaseQuery>
{
    /// <summary>
    /// Defines the validation rules for <see cref="SearchKnowledgeBaseQuery"/>.
    /// </summary>
    public SearchKnowledgeBaseQueryValidator()
    {
        RuleFor(query => query.Question)
            .NotEmpty().WithMessage("A non-empty question is required.");
    }
}

/// <summary>
/// Handler that searches the knowledge base using an embedding of the question.
/// </summary>
public sealed class SearchKnowledgeBaseQueryHandler(
    IEmbeddingService embeddingService,
    IVectorSearchRepository vectorSearchRepository)
    : IRequestHandler<SearchKnowledgeBaseQuery, IReadOnlyList<SearchResultDto>>
{
    private const int DefaultTopK = 5;

    /// <summary>
    /// Searches the knowledge base for chunks relevant to the question.
    /// </summary>
    /// <param name="request">The search query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of relevant knowledge chunks.</returns>
    public async Task<IReadOnlyList<SearchResultDto>> Handle(SearchKnowledgeBaseQuery request, CancellationToken cancellationToken)
    {
        int topK = request.TopK <= 0 ? DefaultTopK : request.TopK;

        float[] queryEmbedding = await embeddingService.GenerateEmbeddingAsync(request.Question, cancellationToken);
        IReadOnlyList<KnowledgeChunkHitDto> hits = await vectorSearchRepository.SearchAsync(queryEmbedding, topK, cancellationToken);

        return hits
            .Select(hit => new SearchResultDto
            {
                Document = hit.DocumentFileName,
                Section = hit.Section,
                ChunkIndex = hit.ChunkIndex,
                Content = hit.Content,
                Score = Math.Round(Math.Clamp(1d - hit.CosineDistance, 0d, 1d), 4)
            })
            .ToList();
    }
}
