using MediatR;
using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Query.SearchKnowledgeBase;

public record SearchKnowledgeBaseQuery(string Question, int TopK = 5) : IRequest<IReadOnlyList<SearchResultDto>>;
