using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Re-ranks a pool of vector search candidates using a hybrid relevance score that combines
/// semantic similarity, keyword overlap, and section relevance.
/// </summary>
public interface IRetrievalRankingService
{
    /// <summary>
    /// Re-ranks the supplied candidates by a blended relevance score and returns the top results.
    /// </summary>
    /// <param name="candidates">The candidate chunks retrieved from vector search, ordered by cosine distance.</param>
    /// <param name="query">The raw user question used for keyword and section relevance scoring.</param>
    /// <param name="topK">The maximum number of results to return after re-ranking.</param>
    /// <returns>The top re-ranked knowledge chunks together with their blended relevance score.</returns>
    IReadOnlyList<SearchResultDto> ReRank(
        IReadOnlyList<KnowledgeChunkHitDto> candidates,
        string query,
        int topK);
}
