using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Resume.Application.Interfaces;

namespace Resume.Infrastructure.AI.Ollama;

internal sealed record ChatMessage(string Role, string Content);

internal sealed record ChatGenerationOptions(float Temperature);

internal sealed record ChatCompletionRequest(
    string Model,
    IReadOnlyList<ChatMessage> Messages,
    bool Stream,
    bool Think,
    ChatGenerationOptions Options);

internal sealed record ChatCompletionResponse(ChatMessage? Message);

public class OllamaLlmService(HttpClient httpClient, IOptions<OllamaOptions> options) : ILlmService
{
    private static readonly Regex ThinkingBlockRegex =
        new("<think>.*?</think>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public async Task<string> GenerateAnswerAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var ollama = options.Value;

        var request = new ChatCompletionRequest(
            ollama.ChatModel,
            new List<ChatMessage>
            {
                new("system", systemPrompt),
                new("user", userPrompt)
            },
            Stream: false,
            Think: false,
            Options: new ChatGenerationOptions(Temperature: 0.2f));

        using var response = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken);
        var content = payload?.Message?.Content ?? string.Empty;

        return ThinkingBlockRegex.Replace(content, string.Empty).Trim();
    }
}
