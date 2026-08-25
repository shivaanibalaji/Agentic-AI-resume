using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Resume.Application.Interfaces;

namespace Resume.Infrastructure.AI.Ollama;

internal sealed record EmbedRequest(string Model, string Input);

internal sealed record EmbedResponse(float[][] Embeddings);

public class OllamaEmbeddingService(HttpClient httpClient, IOptions<OllamaOptions> options) : IEmbeddingService
{
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text must not be null or whitespace.", nameof(text));
        }

        var ollama = options.Value;

        using var response = await httpClient.PostAsJsonAsync(
            "api/embed",
            new EmbedRequest(ollama.EmbeddingModel, text),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken);
        var embedding = payload?.Embeddings is { Length: > 0 } batches ? batches[0] : null;

        if (embedding is null || embedding.Length == 0)
        {
            throw new InvalidOperationException(
                $"Ollama returned no embedding for model '{ollama.EmbeddingModel}'.");
        }

        if (embedding.Length != ollama.EmbeddingDimensions)
        {
            throw new InvalidOperationException(
                $"Embedding model '{ollama.EmbeddingModel}' returned {embedding.Length} dimensions but {ollama.EmbeddingDimensions} are required. " +
                "Vectors are never truncated or padded; configure a model that matches the configured dimension.");
        }

        return embedding;
    }
}
