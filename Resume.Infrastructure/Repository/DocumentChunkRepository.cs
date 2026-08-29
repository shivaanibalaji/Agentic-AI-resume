using Microsoft.EntityFrameworkCore;
using Resume.Application.Interfaces.IRepository;
using Resume.Domain.Entities;
using Resume.Infrastructure.Persistence;

namespace Resume.Infrastructure.Repository;

/// <summary>
/// Provides document chunk data access using Entity Framework Core.
/// </summary>
public class DocumentChunkRepository(ResumeDbContext context) : IDocumentChunkRepository
{
    /// <inheritdoc />
    public Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => context.DocumentChunks
            .Where(chunk => chunk.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);

    /// <inheritdoc />
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
