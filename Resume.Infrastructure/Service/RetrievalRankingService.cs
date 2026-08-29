using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Resume.Application;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.Service;

/// <summary>
/// Re-ranks vector search candidates using a blended relevance score that combines
/// semantic similarity, keyword overlap, a detected resume-section match, and a summary
/// preference for broad questions. Resume questions almost always name their target
/// section ("education", "experience", "projects", "skills"), so matching the candidate's
/// section against the detected intent is a much stronger signal than raw cosine distance,
/// especially with a small embedding model.
///
/// Scoring strategy (see KnowledgeBaseOptions):
///   combined = SemanticWeight * semantic        (0.40) - cosine similarity
///            + KeywordWeight   * keyword        (0.25) - query term overlap with the chunk
///            + SectionWeight   * section        (0.20) - chunk belongs to the detected section
///            + SummaryWeight   * summary        (0.15) - chunk was marked as an overview/summary
///                                                       during ingestion (stored IsSummary
///                                                       metadata) and the question is broad
/// Candidates that do not belong to the detected section are multiplied by
/// CrossSectionPenalty (0.45) so unrelated technical documentation cannot outrank
/// clearly relevant resume content.
/// </summary>
public class RetrievalRankingService(IOptions<KnowledgeBaseOptions> options) : IRetrievalRankingService
{
    private enum ResumeSection
    {
        Education,
        Experience,
        Projects,
        Skills,
        About
    }

    /// <summary>
    /// Matches resume section aliases against the raw user question using whole-word matching
    /// so that words like "work" inside "framework" are not treated as experience signals.
    /// The array order defines detection precedence (Education before About, etc.).
    /// </summary>
    private static readonly KeyValuePair<ResumeSection, IReadOnlyList<string>>[] SectionQueryAliases = new[]
    {
        new KeyValuePair<ResumeSection, IReadOnlyList<string>>(
            ResumeSection.Education,
            new[] { "education", "educational", "academic", "academics", "study", "studies", "studied", "studying", "degree", "degrees", "college", "university", "school", "schooling", "graduation", "graduated", "graduate", "qualification", "qualifications", "b.e.", "bachelor", "undergraduate", "postgraduate", "class", "cgpa", "gpa", "curriculum", "syllabus" }),
        new KeyValuePair<ResumeSection, IReadOnlyList<string>>(
            ResumeSection.Experience,
            new[] { "experience", "experienced", "experiences", "career", "careers", "employment", "employer", "employers", "company", "companies", "organisation", "organization", "organisations", "organizations", "worked", "works", "work", "job", "jobs", "role", "roles", "professional", "position", "positions", "propel" }),
        new KeyValuePair<ResumeSection, IReadOnlyList<string>>(
            ResumeSection.Projects,
            new[] { "project", "projects", "built", "build", "develop", "developed", "development", "creating", "application", "applications", "app", "apps", "portfolio", "portfolios", "kascade", "agentic", "feature", "features" }),
        new KeyValuePair<ResumeSection, IReadOnlyList<string>>(
            ResumeSection.Skills,
            new[] { "skill", "skills", "technology", "technologies", "technical", "stack", "stacks", "know", "knows", "knowledge", "framework", "frameworks", "language", "languages", "library", "libraries", "tech", "tool", "tools" }),
        new KeyValuePair<ResumeSection, IReadOnlyList<string>>(
            ResumeSection.About,
            new[] { "about", "profile", "profiles", "introduction", "background", "who is", "contact" })
    };

    /// <summary>
    /// Matches the source document file name or section heading to a resume section using
    /// substring matching. The document file name is checked first because it is the most
    /// reliable signal (education.md, experience.md, skills.md, about.md, project files).
    /// </summary>
    private static readonly IReadOnlyDictionary<ResumeSection, IReadOnlyList<string>> SectionDocumentMarkers =
        new Dictionary<ResumeSection, IReadOnlyList<string>>
        {
            [ResumeSection.Education] = new[]
            {
                "education", "academic", "school", "study", "degree", "college", "university",
                "graduation", "graduate", "qualification", "bachelor"
            },
            [ResumeSection.Experience] = new[]
            {
                "experience", "career", "job", "employment", "employer", "company", "work",
                "role", "professional", "position"
            },
            [ResumeSection.Projects] = new[]
            {
                "project", "portfolio", "kascade", "agentic", "application", "app"
            },
            [ResumeSection.Skills] = new[]
            {
                "skill", "technolog", "stack", "framework", "language", "tool"
            },
            [ResumeSection.About] = new[]
            {
                "about", "contact", "profile", "introduction", "background"
            }
        };

    /// <summary>
    /// Markers that indicate the question asks about a specific detail (a score, year,
    /// company, class, etc.) rather than requesting a broad overview of a section.
    /// </summary>
    private static readonly string[] SpecificQueryMarkers =
    {
        "score", "scores", "marks", "mark", "cgpa", "gpa", "percentage", "grade", "grades",
        "how many", "how much", "when did", "where did", "when was", "where was", "what year",
        "which year", "in which", "at which", "from which", "which university", "which college",
        "which company", "how long", "class 10", "class 12", "semester", "cgpa of", "number of"
    };

    private static readonly Regex WordValueRegex = new(@"[^\p{L}\p{N}]+", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "or", "but", "of", "in", "on", "at", "to", "for", "with", "from",
        "about", "tell", "me", "her", "his", "their", "she", "he", "they", "them", "what",
        "where", "when", "why", "who", "how", "does", "did", "do", "is", "am", "are", "was",
        "were", "has", "have", "had", "you", "your", "this", "that", "these", "those", "can",
        "could", "would", "should", "will", "please", "give", "as", "by", "it", "its", "not",
        "all", "any", "be", "been", "being", "my", "we", "us", "so", "if", "no", "yes", "go",
        "than", "then", "too", "also", "there", "into", "out", "over", "under"
    };

    /// <inheritdoc />
    public IReadOnlyList<SearchResultDto> ReRank(
        IReadOnlyList<KnowledgeChunkHitDto> candidates,
        string query,
        int topK)
    {
        if (candidates.Count == 0)
        {
            return Array.Empty<SearchResultDto>();
        }

        IReadOnlyList<string> queryTerms = Tokenize(query);
        ResumeSection? intent = DetectQuerySection(query);
        bool broadQuery = IsBroadQuery(query);

        List<(double Score, SearchResultDto Result)> ranked = new List<(double Score, SearchResultDto Result)>(candidates.Count);

        foreach (KnowledgeChunkHitDto hit in candidates)
        {
            double semanticScore = Math.Clamp(1d - hit.CosineDistance, 0d, 1d);
            double keywordScore = ComputeKeywordScore(queryTerms, hit.Section, hit.Content);
            ResumeSection? chunkSection = DetectChunkSection(hit.DocumentFileName, hit.Section);

            bool belongsToIntent = intent.HasValue && chunkSection == intent;
            double sectionScore = belongsToIntent ? 1d : 0d;
            double summaryScore = broadQuery && belongsToIntent && hit.IsSummary ? 1d : 0d;

            double combined =
                (options.Value.SemanticWeight * semanticScore)
                + (options.Value.KeywordWeight * keywordScore)
                + (options.Value.SectionWeight * sectionScore)
                + (options.Value.SummaryWeight * summaryScore);

            if (intent.HasValue && !belongsToIntent)
            {
                combined *= options.Value.CrossSectionPenalty;
            }

            combined = Math.Clamp(combined, 0d, 1d);

            ranked.Add((combined, new SearchResultDto
            {
                Document = hit.DocumentFileName,
                Section = hit.Section,
                ChunkIndex = hit.ChunkIndex,
                Content = hit.Content,
                Score = Math.Round(combined, 4)
            }));
        }

        return ranked
            .OrderByDescending(item => item.Score)
            .Take(topK)
            .Select(item => item.Result)
            .ToList();
    }

    private static ResumeSection? DetectQuerySection(string query)
    {
        string lower = query.ToLowerInvariant();

        foreach (KeyValuePair<ResumeSection, IReadOnlyList<string>> aliasEntry in SectionQueryAliases)
        {
            if (aliasEntry.Value.Any(alias => ContainsAsWord(lower, alias)))
            {
                return aliasEntry.Key;
            }
        }

        return null;
    }

    private static ResumeSection? DetectChunkSection(string documentFileName, string sectionHeading)
    {
        foreach (KeyValuePair<ResumeSection, IReadOnlyList<string>> markerEntry in SectionDocumentMarkers)
        {
            if (markerEntry.Value.Any(marker => documentFileName.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                return markerEntry.Key;
            }
        }

        foreach (KeyValuePair<ResumeSection, IReadOnlyList<string>> markerEntry in SectionDocumentMarkers)
        {
            if (markerEntry.Value.Any(marker => sectionHeading.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                return markerEntry.Key;
            }
        }

        return null;
    }

    private static double ComputeKeywordScore(IReadOnlyList<string> queryTerms, string section, string content)
    {
        if (queryTerms.Count == 0)
        {
            return 0d;
        }

        HashSet<string> textTerms = new HashSet<string>(Tokenize($"{section} {content}"), StringComparer.OrdinalIgnoreCase);
        int matched = queryTerms.Count(term => textTerms.Contains(term));

        return (double)matched / queryTerms.Count;
    }

    private static bool IsBroadQuery(string query)
    {
        string lower = query.ToLowerInvariant();
        return !SpecificQueryMarkers.Any(marker => lower.Contains(marker, StringComparison.Ordinal));
    }

    private static bool ContainsAsWord(string text, string word)
        => Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);

    private static IReadOnlyList<string> Tokenize(string text)
        => WordValueRegex
            .Split(text)
            .Where(token => token.Length >= 2 && !StopWords.Contains(token))
            .Select(token => token.ToLowerInvariant())
            .ToList();
}