namespace Resume.Infrastructure.AI.Ollama;

/// <summary>
/// Configuration options for the Ollama integration.
/// </summary>
public class OllamaOptions
{
    /// <summary>
    /// The configuration section name used to bind these options.
    /// </summary>
    public const string SectionName = "Ollama";

    /// <summary>
    /// Gets or sets the base URL of the Ollama server.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets the name of the embedding model.
    /// </summary>
    public string EmbeddingModel { get; set; } = "qwen3-embedding:0.6b";

    /// <summary>
    /// Gets or sets the number of embedding dimensions expected for the model.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the name of the chat model.
    /// </summary>
    public string ChatModel { get; set; } = "qwen3:8b";
}
