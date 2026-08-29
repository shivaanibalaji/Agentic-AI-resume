using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Loads markdown documents from the configured knowledge base directory.
/// </summary>
public interface IMarkdownDocumentLoaderService
{
    /// <summary>
    /// Loads all markdown documents from the knowledge base directory.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of loaded markdown documents.</returns>
    Task<IReadOnlyList<MarkdownDocumentDto>> LoadAsync(CancellationToken cancellationToken = default);
}
