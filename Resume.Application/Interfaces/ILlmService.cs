namespace Resume.Application.Interfaces;

public interface ILlmService
{
    Task<string> GenerateAnswerAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
