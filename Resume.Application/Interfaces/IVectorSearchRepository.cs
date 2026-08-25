namespace Resume.Application.Interfaces;

public sealed record KnowledgeChunkHit(
    string DocumentFileName,
    string Section,
    int ChunkIndex,
    string Content,
    double CosineDistance);

public interface IVectorSearchRepository
{
    Task<IReadOnlyList<KnowledgeChunkHit>> SearchAsync(float[] queryEmbedding, int topK, CancellationToken cancellationToken = default);
}
