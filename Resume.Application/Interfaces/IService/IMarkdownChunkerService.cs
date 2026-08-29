using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Splits markdown documents into smaller chunks suitable for embedding and search.
/// </summary>
public interface IMarkdownChunkerService
{
    /// <summary>
    /// Chunks the supplied markdown document into smaller pieces.
    /// </summary>
    /// <param name="document">The markdown document to chunk.</param>
    /// <returns>A collection of chunks extracted from the document.</returns>
    IReadOnlyList<MarkdownChunkDto> Chunk(MarkdownDocumentDto document);
}
