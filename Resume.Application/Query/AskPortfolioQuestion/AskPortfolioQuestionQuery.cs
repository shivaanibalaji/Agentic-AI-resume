using MediatR;
using Resume.Application.DTO.Chat;

namespace Resume.Application.Query.AskPortfolioQuestion;

public record AskPortfolioQuestionQuery(string Message, int TopK = 5) : IRequest<ChatResponseDto>;
