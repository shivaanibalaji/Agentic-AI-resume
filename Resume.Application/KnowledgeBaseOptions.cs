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

    /// <summary>
    /// Gets or sets the size of the candidate pool retrieved from vector search before
    /// hybrid re-ranking is applied. A larger pool lets topically relevant chunks surface
    /// even when they rank slightly below the top results by pure cosine distance.
    /// </summary>
    public int CandidatePoolSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the weight applied to the semantic (cosine similarity) score during
    /// hybrid re-ranking. Kept highest but reduced alongside the other weights because a
    /// small embedding model alone cannot reliably separate resume topics, so the lexical
    /// and structural signals must carry more of the ranking responsibility.
    /// </summary>
    public double SemanticWeight { get; set; } = 0.40;

    /// <summary>
    /// Gets or sets the weight applied to the keyword overlap score during hybrid re-ranking.
    /// Rewards chunks that actually contain the words used in the question, which is what
    /// surfaces the specific detail chunk (for example Class 10 vs Class 12).
    /// </summary>
    public double KeywordWeight { get; set; } = 0.25;

    /// <summary>
    /// Gets or sets the weight applied when the chunk belongs to the resume section detected
    /// from the question (Education, Experience, Projects, Skills, About). This is a binary
    /// category match rather than raw heading overlap, so every chunk from the detected
    /// section receives an equal advantage over unrelated documentation.
    /// </summary>
    public double SectionWeight { get; set; } = 0.20;

    /// <summary>
    /// Gets or sets the weight applied to overview/summary chunks when the question is a
    /// broad section query (for example "Tell me about her education"). This ensures a
    /// summary chunk such as Education Summary is preferred over a specific detail chunk
    /// such as Secondary Education - Class 10 for broad questions, while the keyword score
    /// still lets specific questions surface their specific chunk.
    /// </summary>
    public double SummaryWeight { get; set; } = 0.15;

    /// <summary>
    /// Gets or sets the multiplicative penalty applied to candidates that do not belong to
    /// the section detected from the question. Keeps unrelated technical documentation
    /// (for example Redis or project architecture notes) from outranking clearly relevant
    /// resume content without hard-excluding other sections.
    /// </summary>
    public double CrossSectionPenalty { get; set; } = 0.45;
}
