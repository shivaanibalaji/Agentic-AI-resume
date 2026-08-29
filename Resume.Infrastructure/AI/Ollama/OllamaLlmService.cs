using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.AI.Ollama;

/// <summary>
/// Generates natural language answers using the Ollama chat API.
/// </summary>
public class OllamaLlmService(HttpClient httpClient, IOptions<OllamaOptions> options) : ILlmService
{
    private static readonly Regex ThinkingBlockRegex =
        new(" thinking.*? response", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <inheritdoc />
    public async Task<string> GenerateAnswerAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        OllamaOptions ollama = options.Value;

        ChatCompletionRequest request = new ChatCompletionRequest(
            ollama.ChatModel,
            new List<ChatMessage>
            {
                new("system", systemPrompt),
                new("user", userPrompt)
            },
            Stream: false,
            Think: false,
            Options: new ChatGenerationOptions(Temperature: 0.2f));

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        ChatCompletionResponse? payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken);
        string content = payload?.Message?.Content ?? string.Empty;

        return ThinkingBlockRegex.Replace(content, string.Empty).Trim();
    }

    private sealed record ChatMessage(string Role, string Content);

    private sealed record ChatGenerationOptions(float Temperature);

    private sealed record ChatCompletionRequest(
        string Model,
        IReadOnlyList<ChatMessage> Messages,
        bool Stream,
        bool Think,
        ChatGenerationOptions Options);

    private sealed record ChatCompletionResponse(ChatMessage? Message);
}
