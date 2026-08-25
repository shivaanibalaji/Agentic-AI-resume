using MediatR;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces;

namespace Resume.Application.Query.SearchKnowledgeBase;

public class SearchKnowledgeBaseQueryHandler(
    IEmbeddingService embeddingService,
    IVectorSearchRepository vectorSearchRepository)
    : IRequestHandler<SearchKnowledgeBaseQuery, IReadOnlyList<SearchResultDto>>
{
    private const int DefaultTopK = 5;

    public async Task<IReadOnlyList<SearchResultDto>> Handle(SearchKnowledgeBaseQuery request, CancellationToken cancellationToken)
    {
        var topK = request.TopK <= 0 ? DefaultTopK : request.TopK;

        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(request.Question, cancellationToken);
        var hits = await vectorSearchRepository.SearchAsync(queryEmbedding, topK, cancellationToken);

        return hits
            .Select(hit => new SearchResultDto(
                hit.DocumentFileName,
                hit.Section,
                hit.ChunkIndex,
                hit.Content,
                Math.Round(Math.Clamp(1d - hit.CosineDistance, 0d, 1d), 4)))
            .ToList();
    }
}
