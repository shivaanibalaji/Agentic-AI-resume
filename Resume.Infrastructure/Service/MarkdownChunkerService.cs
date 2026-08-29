using System.Text;
using Microsoft.Extensions.Options;
using Resume.Application;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.Service;

/// <summary>
/// Splits markdown documents into smaller chunks suitable for embedding and search.
/// </summary>
public class MarkdownChunkerService(IOptions<KnowledgeBaseOptions> options) : IMarkdownChunkerService
{
    private readonly int _chunkSize = Math.Max(options.Value.ChunkSize, 100);
    private readonly int _chunkOverlap = Math.Clamp(options.Value.ChunkOverlap, 0, Math.Max(options.Value.ChunkSize / 2, 1));

    /// <summary>
    /// Generic, content-agnostic words that indicate a section is an overview, summary, or
    /// introduction of its document. Not tied to any specific knowledge-base topic, so it
    /// applies uniformly as new documents and sections are added.
    /// </summary>
    private static readonly string[] OverviewHeadingMarkers =
    {
        "summary", "overview", "introduction", "background", "profile", "objective", "highlights", "journey"
    };

    /// <inheritdoc />
    public IReadOnlyList<MarkdownChunkDto> Chunk(MarkdownDocumentDto document)
    {
        List<MarkdownChunkDto> chunks = new List<MarkdownChunkDto>();
        int chunkIndex = 0;

        foreach ((string Heading, string Content) section in SplitIntoSections(document.Content))
        {
            bool isSummary = IsOverviewHeading(section.Heading);

            foreach (string piece in SplitSection(section.Content))
            {
                chunks.Add(new MarkdownChunkDto
                {
                    Section = section.Heading,
                    ChunkIndex = chunkIndex++,
                    Content = piece,
                    IsSummary = isSummary
                });
            }
        }

        return chunks;
    }

    private static bool IsOverviewHeading(string heading)
    {
        string lower = heading.ToLowerInvariant();
        return OverviewHeadingMarkers.Any(marker => lower.Contains(marker, StringComparison.Ordinal));
    }

    private static List<(string Heading, string Content)> SplitIntoSections(string content)
    {
        List<(string Heading, string Content)> sections = new List<(string Heading, string Content)>();
        string currentHeading = "(Introduction)";
        StringBuilder body = new StringBuilder();

        foreach (string rawLine in content.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');

            if (TryGetHeading(line, out string heading))
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
        string content = body.ToString().Trim();

        if (content.Length > 0)
        {
            sections.Add((heading, content));
        }

        body.Clear();
    }

    private static bool TryGetHeading(string line, out string heading)
    {
        heading = string.Empty;

        string trimmed = line.Trim();

        if (trimmed.Length == 0 || trimmed[0] != '#')
        {
            return false;
        }

        int hashes = 0;

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

        int start = 0;

        while (start < content.Length)
        {
            int end = Math.Min(start + _chunkSize, content.Length);

            if (end < content.Length)
            {
                int boundary = content.LastIndexOf("\n\n", end - 1, end - start, StringComparison.Ordinal);

                if (boundary > start + (_chunkSize / 2))
                {
                    end = boundary + 2;
                }
            }

            string piece = content[start..end].Trim();

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
