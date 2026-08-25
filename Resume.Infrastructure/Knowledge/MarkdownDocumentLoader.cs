using Microsoft.Extensions.Options;
using Resume.Application;
using Resume.Application.Interfaces;

namespace Resume.Infrastructure.Knowledge;

public class MarkdownDocumentLoader(IOptions<KnowledgeBaseOptions> options) : IMarkdownDocumentLoader
{
    private readonly KnowledgeBaseOptions _options = options.Value;
    public Task<IReadOnlyList<MarkdownDocument>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var directory = _options.Path;

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Knowledge base directory not found: '{Path.GetFullPath(directory)}'. " +
                "Create it and add markdown files, or update KnowledgeBase:Path.");
        }

        var documents = new List<MarkdownDocument>();

        foreach (var filePath in Directory.EnumerateFiles(directory, "*.md").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var content = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var fileName = Path.GetFileName(filePath);
            documents.Add(new MarkdownDocument(fileName, ExtractTitle(content, fileName), content.Trim()));
        }

        return Task.FromResult<IReadOnlyList<MarkdownDocument>>(documents);
    }

    private static string ExtractTitle(string content, string fileName)
    {
        using var reader = new StringReader(content);

        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.TrimStart();

            if (!trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            var title = trimmed[2..].Trim().TrimEnd('#').Trim();

            if (title.Length > 0)
            {
                return title;
            }
        }

        return Path.GetFileNameWithoutExtension(fileName);
    }
}
