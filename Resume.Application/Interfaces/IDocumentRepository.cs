using Resume.Domain.Entities;

namespace Resume.Application.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default);

    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
}
