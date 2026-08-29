using Microsoft.Extensions.Options;
using Resume.Application;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.Service;

/// <summary>
/// Loads markdown documents from the configured knowledge base directory.
/// </summary>
public class MarkdownDocumentLoaderService(IOptions<KnowledgeBaseOptions> options) : IMarkdownDocumentLoaderService
{
    private readonly KnowledgeBaseOptions _options = options.Value;

    /// <inheritdoc />
    public Task<IReadOnlyList<MarkdownDocumentDto>> LoadAsync(CancellationToken cancellationToken = default)
    {
        string directory = _options.Path;

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Knowledge base directory not found: '{Path.GetFullPath(directory)}'. " +
                "Create it and add markdown files, or update KnowledgeBase:Path.");
        }

        List<MarkdownDocumentDto> documents = new List<MarkdownDocumentDto>();

        foreach (string filePath in Directory.EnumerateFiles(directory, "*.md", SearchOption.AllDirectories)
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
        {
            string content = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            string fileName = Path.GetFileName(filePath);
            documents.Add(new MarkdownDocumentDto
            {
                FileName = fileName,
                Title = ExtractTitle(content, fileName),
                Content = content.Trim()
            });
        }

        return Task.FromResult<IReadOnlyList<MarkdownDocumentDto>>(documents);
    }

    private static string ExtractTitle(string content, string fileName)
    {
        using StringReader reader = new StringReader(content);

        while (reader.ReadLine() is { } line)
        {
            string trimmed = line.TrimStart();

            if (!trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            string title = trimmed[2..].Trim().TrimEnd('#').Trim();

            if (title.Length > 0)
            {
                return title;
            }
        }

        return Path.GetFileNameWithoutExtension(fileName);
    }
}
