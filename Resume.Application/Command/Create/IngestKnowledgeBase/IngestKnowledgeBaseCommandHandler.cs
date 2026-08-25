using System.Text.Json;
using MediatR;
using Pgvector;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces;
using Resume.Domain.Entities;

namespace Resume.Application.Command.Create.IngestKnowledgeBase;

public class IngestKnowledgeBaseCommandHandler(
    IMarkdownDocumentLoader markdownDocumentLoader,
    IMarkdownChunker markdownChunker,
    IEmbeddingService embeddingService,
    IDocumentRepository documentRepository,
    IDocumentChunkRepository documentChunkRepository)
    : IRequestHandler<IngestKnowledgeBaseCommand, IngestionResultDto>
{
    public async Task<IngestionResultDto> Handle(IngestKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var sources = await markdownDocumentLoader.LoadAsync(cancellationToken);

        var newDocuments = 0;
        var updatedDocuments = 0;
        var totalChunks = 0;

        foreach (var source in sources)
        {
            var document = await documentRepository.GetByFileNameAsync(source.FileName, cancellationToken);

            if (document is null)
            {
                document = new Document
                {
                    FileName = source.FileName,
                    Title = source.Title,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await documentRepository.AddAsync(document, cancellationToken);
                newDocuments++;
            }
            else if (document.Title != source.Title)
            {
                document.Title = source.Title;
                document.UpdatedAt = DateTime.UtcNow;
                await documentRepository.UpdateAsync(document, cancellationToken);
                updatedDocuments++;
            }

            await documentChunkRepository.DeleteByDocumentIdAsync(document.Id, cancellationToken);

            var chunks = markdownChunker.Chunk(source);
            var entities = new List<DocumentChunk>(chunks.Count);

            foreach (var chunk in chunks)
            {
                var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Content, cancellationToken);

                entities.Add(new DocumentChunk
                {
                    DocumentId = document.Id,
                    Content = chunk.Content,
                    Section = chunk.Section,
                    ChunkIndex = chunk.ChunkIndex,
                    Embedding = new Vector(embedding),
                    Metadata = JsonSerializer.Serialize(new { characters = chunk.Content.Length }),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await documentChunkRepository.AddRangeAsync(entities, cancellationToken);
            totalChunks += entities.Count;
        }

        return new IngestionResultDto(sources.Count, newDocuments, updatedDocuments, totalChunks);
    }
}
