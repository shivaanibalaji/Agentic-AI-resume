using Resume.Application.DTO.JobAnalyzer;
using Resume.Application.DTO.Knowledge;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.Service;

/// <summary>
/// Matches an individual job requirement against the portfolio knowledge base using
/// embeddings and vector search, then classifies the match deterministically.
/// </summary>
public class JobRequirementMatcherService(
    IEmbeddingService embeddingService,
    IVectorSearchRepository vectorSearchRepository)
    : IJobRequirementMatcherService
{
    private const int DefaultTopK = 3;

    private const double StrongThreshold = 0.60;
    private const double PartialThreshold = 0.35;

    /// <inheritdoc />
    public async Task<RequirementMatchDto> MatchAsync(string requirement, CancellationToken cancellationToken = default)
    {
        float[] requirementEmbedding = await embeddingService.GenerateEmbeddingAsync(requirement, cancellationToken);
        IReadOnlyList<KnowledgeChunkHitDto> hits = await vectorSearchRepository.SearchAsync(
            requirementEmbedding,
            DefaultTopK,
            cancellationToken);

        List<PortfolioContentDto> matchingContent = hits
            .Select(hit => new PortfolioContentDto
            {
                Document = hit.DocumentFileName,
                Section = hit.Section,
                ChunkIndex = hit.ChunkIndex,
                Content = hit.Content,
                Score = Math.Round(Math.Clamp(1d - hit.CosineDistance, 0d, 1d), 4)
            })
            .OrderByDescending(content => content.Score)
            .ToList();

        MatchStatus status = Classify(matchingContent);

        return new RequirementMatchDto
        {
            Requirement = requirement,
            Status = status.ToString(),
            MatchingContent = matchingContent
        };
    }

    private static MatchStatus Classify(IReadOnlyList<PortfolioContentDto> matchingContent)
    {
        if (matchingContent.Count == 0)
        {
            return MatchStatus.Weak;
        }

        double bestScore = matchingContent[0].Score;

        if (bestScore >= StrongThreshold)
        {
            return MatchStatus.Strong;
        }

        if (bestScore >= PartialThreshold)
        {
            return MatchStatus.Partial;
        }

        return MatchStatus.Weak;
    }
}
