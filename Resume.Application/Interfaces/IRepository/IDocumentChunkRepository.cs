using Resume.Domain.Entities;

namespace Resume.Application.Interfaces.IRepository;

/// <summary>
/// Provides data access operations for document chunks.
/// </summary>
public interface IDocumentChunkRepository
{
    /// <summary>
    /// Deletes all chunks belonging to the specified document.
    /// </summary>
    /// <param name="documentId">The identifier of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a range of document chunks to the store.
    /// </summary>
    /// <param name="chunks">The chunks to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IReadOnlyCollection<DocumentChunk> chunks, CancellationToken cancellationToken = default);
}
