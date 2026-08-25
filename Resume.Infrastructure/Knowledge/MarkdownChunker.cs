using System.Text;
using Microsoft.Extensions.Options;
using Resume.Application;
using Resume.Application.Interfaces;

namespace Resume.Infrastructure.Knowledge;

public class MarkdownChunker(IOptions<KnowledgeBaseOptions> options) : IMarkdownChunker
{
    private readonly int _chunkSize = Math.Max(options.Value.ChunkSize, 100);
    private readonly int _chunkOverlap = Math.Clamp(options.Value.ChunkOverlap, 0, Math.Max(options.Value.ChunkSize / 2, 1));

    public IReadOnlyList<MarkdownChunk> Chunk(MarkdownDocument document)
    {
        var chunks = new List<MarkdownChunk>();
        var chunkIndex = 0;

        foreach (var section in SplitIntoSections(document.Content))
        {
            foreach (var piece in SplitSection(section.Content))
            {
                chunks.Add(new MarkdownChunk(section.Heading, chunkIndex++, piece));
            }
        }

        return chunks;
    }

    private static List<(string Heading, string Content)> SplitIntoSections(string content)
    {
        var sections = new List<(string Heading, string Content)>();
        var currentHeading = "(Introduction)";
        var body = new StringBuilder();

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (TryGetHeading(line, out var heading))
            {
                AddSection(sections, currentHeading, body);
                currentHeading = heading;
            }
            else
            {
                body.AppendLine(line);
            }
        }

        AddSection(sections, currentHeading, body);

        return sections;
    }

    private static void AddSection(List<(string Heading, string Content)> sections, string heading, StringBuilder body)
    {
        var content = body.ToString().Trim();

        if (content.Length > 0)
        {
            sections.Add((heading, content));
        }

        body.Clear();
    }

    private static bool TryGetHeading(string line, out string heading)
    {
        heading = string.Empty;

        var trimmed = line.Trim();

        if (trimmed.Length == 0 || trimmed[0] != '#')
        {
            return false;
        }

        var hashes = 0;

        while (hashes < trimmed.Length && trimmed[hashes] == '#')
        {
            hashes++;
        }

        if (hashes > 6 || hashes >= trimmed.Length || !char.IsWhiteSpace(trimmed[hashes]))
        {
            return false;
        }

        heading = trimmed[(hashes + 1)..].Trim().TrimEnd('#').Trim();

        return heading.Length > 0;
    }

    private IEnumerable<string> SplitSection(string content)
    {
        if (content.Length <= _chunkSize)
        {
            yield return content;
            yield break;
        }

        var start = 0;

        while (start < content.Length)
        {
            var end = Math.Min(start + _chunkSize, content.Length);

            if (end < content.Length)
            {
                var boundary = content.LastIndexOf("\n\n", end - 1, end - start, StringComparison.Ordinal);

                if (boundary > start + (_chunkSize / 2))
                {
                    end = boundary + 2;
                }
            }

            var piece = content[start..end].Trim();

            if (piece.Length > 0)
            {
                yield return piece;
            }

            if (end >= content.Length)
            {
                yield break;
            }

            start = Math.Max(end - _chunkOverlap, start + 1);
        }
    }
}
