namespace Resume.Application.Interfaces.IService;

/// <summary>
/// Generates natural language answers using a language model.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Generates an answer using the supplied system and user prompts.
    /// </summary>
    /// <param name="systemPrompt">The system prompt that guides the model.</param>
    /// <param name="userPrompt">The user prompt containing the question and context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated answer.</returns>
    Task<string> GenerateAnswerAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
