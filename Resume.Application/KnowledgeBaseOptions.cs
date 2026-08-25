namespace Resume.Application;

public class KnowledgeBaseOptions
{
    public const string SectionName = "KnowledgeBase";

    public string Path { get; set; } = "../Knowledge";

    public int ChunkSize { get; set; } = 1000;

    public int ChunkOverlap { get; set; } = 150;
}
