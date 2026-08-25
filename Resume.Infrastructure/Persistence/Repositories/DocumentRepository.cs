using Microsoft.EntityFrameworkCore;
using Resume.Application.Interfaces;
using Resume.Domain.Entities;

namespace Resume.Infrastructure.Persistence.Repositories;

public class DocumentRepository(ResumeDbContext context) : IDocumentRepository
{
    public Task<Document?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
        => context.Documents.FirstOrDefaultAsync(d => d.FileName == fileName, cancellationToken);

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await context.Documents.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        context.Documents.Update(document);
        await context.SaveChangesAsync(cancellationToken);
    }
}
