using System.Text.Json;
using MediatR;
using Pgvector;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;
using Resume.Domain.Entities;

namespace Resume.Application.Command.Create.IngestKnowledgeBase;

/// <summary>
/// Command that ingests markdown documents from the knowledge base directory into
/// the vector database (creating, updating, and deleting chunks as needed).
/// </summary>
public sealed record IngestKnowledgeBaseCommand : IRequest<IngestionResultDto>;

/// <summary>
/// Handler that performs the knowledge base ingestion workflow.
/// </summary>
public sealed class IngestKnowledgeBaseCommandHandler(
    IMarkdownDocumentLoaderService markdownDocumentLoader,
    IMarkdownChunkerService markdownChunker,
    IEmbeddingService embeddingService,
    IDocumentRepository documentRepository,
    IDocumentChunkRepository documentChunkRepository)
    : IRequestHandler<IngestKnowledgeBaseCommand, IngestionResultDto>
{
    /// <summary>
    /// Ingests all markdown documents found in the configured knowledge base directory.
    /// </summary>
    /// <param name="request">The ingestion command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result describing the number of files, documents, and chunks processed.</returns>
    public async Task<IngestionResultDto> Handle(IngestKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<MarkdownDocumentDto> sources = await markdownDocumentLoader.LoadAsync(cancellationToken);

        int newDocuments = 0;
        int updatedDocuments = 0;
        int totalChunks = 0;

        foreach (MarkdownDocumentDto source in sources)
        {
            Document? document = await documentRepository.GetByFileNameAsync(source.FileName, cancellationToken);

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

            IReadOnlyList<MarkdownChunkDto> chunks = markdownChunker.Chunk(source);
            List<DocumentChunk> entities = new List<DocumentChunk>(chunks.Count);

            foreach (MarkdownChunkDto chunk in chunks)
            {
                float[] embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Content, cancellationToken);

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

        return new IngestionResultDto
        {
            TotalFiles = sources.Count,
            NewDocuments = newDocuments,
            UpdatedDocuments = updatedDocuments,
            TotalChunks = totalChunks
        };
    }
}
