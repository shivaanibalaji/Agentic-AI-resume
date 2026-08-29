namespace Resume.Application;

/// <summary>
/// Configuration options for the knowledge base.
/// </summary>
public class KnowledgeBaseOptions
{
    /// <summary>
    /// The configuration section name used to bind these options.
    /// </summary>
    public const string SectionName = "KnowledgeBase";

    /// <summary>
    /// Gets or sets the path to the knowledge base directory.
    /// </summary>
    public string Path { get; set; } = "../Knowledge";

    /// <summary>
    /// Gets or sets the target chunk size in characters.
    /// </summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the chunk overlap in characters.
    /// </summary>
    public int ChunkOverlap { get; set; } = 150;
}
