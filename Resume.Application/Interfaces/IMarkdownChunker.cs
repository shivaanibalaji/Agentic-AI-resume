namespace Resume.Application.Interfaces;

public sealed record MarkdownChunk(string Section, int ChunkIndex, string Content);

public interface IMarkdownChunker
{
    IReadOnlyList<MarkdownChunk> Chunk(MarkdownDocument document);
}
