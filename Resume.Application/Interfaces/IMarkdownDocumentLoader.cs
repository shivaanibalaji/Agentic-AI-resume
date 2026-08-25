namespace Resume.Application.Interfaces;

public sealed record MarkdownDocument(string FileName, string Title, string Content);

public interface IMarkdownDocumentLoader
{
    Task<IReadOnlyList<MarkdownDocument>> LoadAsync(CancellationToken cancellationToken = default);
}
