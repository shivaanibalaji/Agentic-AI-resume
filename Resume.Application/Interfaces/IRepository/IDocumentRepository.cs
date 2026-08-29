using Resume.Domain.Entities;

namespace Resume.Application.Interfaces.IRepository;

/// <summary>
/// Provides data access operations for documents.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Retrieves a document by its file name, or null if none exists.
    /// </summary>
    /// <param name="fileName">The file name of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching document, or null if not found.</returns>
    Task<Document?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all documents in the store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all documents.</returns>
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new document to the store.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added document.</returns>
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document in the store.
    /// </summary>
    /// <param name="document">The document to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document and its related chunks from the store.
    /// </summary>
    /// <param name="document">The document to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Document document, CancellationToken cancellationToken = default);
}
