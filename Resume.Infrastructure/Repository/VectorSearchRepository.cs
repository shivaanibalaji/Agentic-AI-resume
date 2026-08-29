using Microsoft.EntityFrameworkCore;
using Pgvector;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Infrastructure.Persistence;

namespace Resume.Infrastructure.Repository;

/// <summary>
/// Provides vector-based similarity search over knowledge chunks.
/// </summary>
public class VectorSearchRepository(ResumeDbContext context) : IVectorSearchRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<KnowledgeChunkHitDto>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
        => await SearchWithLimitAsync(queryEmbedding, topK, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<KnowledgeChunkHitDto>> SearchCandidatesAsync(
        float[] queryEmbedding,
        int candidateCount,
        CancellationToken cancellationToken = default)
        => await SearchWithLimitAsync(queryEmbedding, candidateCount, cancellationToken);

    private async Task<IReadOnlyList<KnowledgeChunkHitDto>> SearchWithLimitAsync(
        float[] queryEmbedding,
        int limit,
        CancellationToken cancellationToken)
    {
        Vector queryVector = new Vector(queryEmbedding);

        List<KnowledgeChunkHitDto> rows = await context.Database
            .SqlQuery<KnowledgeChunkHitDto>($"""
                SELECT d."FileName" AS "DocumentFileName",
                       c."Section" AS "Section",
                       c."ChunkIndex" AS "ChunkIndex",
                       c."Content" AS "Content",
                       c."Embedding" <=> {queryVector} AS "CosineDistance",
                       c."IsSummary" AS "IsSummary"
                FROM "DocumentChunks" AS c
                JOIN "Documents" AS d ON d."Id" = c."DocumentId"
                ORDER BY c."Embedding" <=> {queryVector}
                LIMIT {limit}
                """)
            .ToListAsync(cancellationToken);

        return rows;
    }
}
