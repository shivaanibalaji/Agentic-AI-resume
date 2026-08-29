using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Interfaces.IRepository;

/// <summary>
/// Provides vector-based similarity search over knowledge chunks.
/// </summary>
public interface IVectorSearchRepository
{
    /// <summary>
    /// Searches for the knowledge chunks most relevant to the supplied query embedding.
    /// </summary>
    /// <param name="queryEmbedding">The embedding of the search query.</param>
    /// <param name="topK">The maximum number of results to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of relevant knowledge chunks.</returns>
    Task<IReadOnlyList<KnowledgeChunkHitDto>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for a broader pool of candidate knowledge chunks ordered by cosine distance.
    /// This wider pool is intended to be re-ranked by a downstream relevance service so that
    /// topically relevant chunks can surface even when they rank slightly below the top results.
    /// </summary>
    /// <param name="queryEmbedding">The embedding of the search query.</param>
    /// <param name="candidateCount">The number of candidate chunks to retrieve from the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of candidate knowledge chunks ordered by ascending cosine distance.</returns>
    Task<IReadOnlyList<KnowledgeChunkHitDto>> SearchCandidatesAsync(
        float[] queryEmbedding,
        int candidateCount,
        CancellationToken cancellationToken = default);
}
