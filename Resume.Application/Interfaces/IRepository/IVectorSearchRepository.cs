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
}
