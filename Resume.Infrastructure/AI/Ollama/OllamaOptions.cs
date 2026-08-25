namespace Resume.Infrastructure.AI.Ollama;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string EmbeddingModel { get; set; } = "qwen3-embedding:0.6b";

    public int EmbeddingDimensions { get; set; } = 1024;

    public string ChatModel { get; set; } = "qwen3:8b";
}
