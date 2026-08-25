using Microsoft.EntityFrameworkCore;
using Resume.Application.Interfaces;
using Resume.Domain.Entities;

namespace Resume.Infrastructure.Persistence.Repositories;

public class DocumentChunkRepository(ResumeDbContext context) : IDocumentChunkRepository
{
    public Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => context.DocumentChunks
            .Where(chunk => chunk.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        await context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
