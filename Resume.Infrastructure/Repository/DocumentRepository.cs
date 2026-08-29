using Microsoft.EntityFrameworkCore;
using Resume.Application.Interfaces.IRepository;
using Resume.Domain.Entities;
using Resume.Infrastructure.Persistence;

namespace Resume.Infrastructure.Repository;

/// <summary>
/// Provides document data access using Entity Framework Core.
/// </summary>
public class DocumentRepository(ResumeDbContext context) : IDocumentRepository
{
    /// <inheritdoc />
    public Task<Document?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
        => context.Documents.FirstOrDefaultAsync(document => document.FileName == fileName, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Documents.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await context.Documents.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return document;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        context.Documents.Update(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
    {
        context.Documents.Remove(document);
        await context.SaveChangesAsync(cancellationToken);
    }
}
