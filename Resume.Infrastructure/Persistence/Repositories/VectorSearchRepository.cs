using Microsoft.EntityFrameworkCore;
using Pgvector;
using Resume.Application.Interfaces;

namespace Resume.Infrastructure.Persistence.Repositories;

public class VectorSearchRepository(ResumeDbContext context) : IVectorSearchRepository
{
    public async Task<IReadOnlyList<KnowledgeChunkHit>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        var queryVector = new Vector(queryEmbedding);

        var rows = await context.Database
            .SqlQuery<KnowledgeChunkHitRow>($"""
                SELECT d."FileName" AS "DocumentFileName",
                       c."Section" AS "Section",
                       c."ChunkIndex" AS "ChunkIndex",
                       c."Content" AS "Content",
                       c."Embedding" <=> {queryVector} AS "CosineDistance"
                FROM "DocumentChunks" AS c
                JOIN "Documents" AS d ON d."Id" = c."DocumentId"
                ORDER BY c."Embedding" <=> {queryVector}
                LIMIT {topK}
                """)
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new KnowledgeChunkHit(
                row.DocumentFileName,
                row.Section,
                row.ChunkIndex,
                row.Content,
                row.CosineDistance))
            .ToList();
    }

    private sealed record KnowledgeChunkHitRow(
        string DocumentFileName,
        string Section,
        int ChunkIndex,
        string Content,
        double CosineDistance);
}
