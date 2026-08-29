namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Generates embeddings for text using an embedding provider.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding for the supplied text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The embedding vector for the text.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
