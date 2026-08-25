using Resume.Domain.Entities;

namespace Resume.Application.Interfaces;

public interface IDocumentChunkRepository
{
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyCollection<DocumentChunk> chunks, CancellationToken cancellationToken = default);
}
