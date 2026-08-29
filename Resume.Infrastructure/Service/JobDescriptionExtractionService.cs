using System.Text.Json;
using Resume.Application.DTO.JobAnalyzer;
using Resume.Application.Interfaces.IService;

namespace Resume.Infrastructure.Service;

/// <summary>
/// Extracts structured requirements from a job description using the Ollama language model.
/// </summary>
public class JobDescriptionExtractionService(ILlmService llmService) : IJobDescriptionExtractionService
{
    private const string SystemPrompt =
        "You extract structured information from job descriptions. " +
        "Respond only with a single JSON object and no additional text.";

    /// <inheritdoc />
    public async Task<JobDescriptionExtractionDto> ExtractAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        string userPrompt = $$"""
            Extract the following information from this job description:
            - "skills": an array of strings listing the required skills.
            - "requirements": an array of strings, one string per individual requirement mentioned in the job description (responsibilities and qualifications).
            - "technologies": an array of strings listing the technologies, tools, and frameworks mentioned.
            - "experience": a short string summarizing the required level of experience.

            Respond with a single JSON object in exactly this shape:
            { "skills": [...], "requirements": [...], "technologies": [...], "experience": "..." }

            Job description:
            {{jobDescription}}
            """;

        string raw = await llmService.GenerateAnswerAsync(SystemPrompt, userPrompt, cancellationToken);

        return ParseExtraction(raw);
    }

    private static JobDescriptionExtractionDto ParseExtraction(string raw)
    {
        string json = ExtractJsonObject(raw);

        try
        {
            ExtractedPayload? payload = JsonSerializer.Deserialize<ExtractedPayload>(json);

            if (payload is null)
            {
                return new JobDescriptionExtractionDto();
            }

            return new JobDescriptionExtractionDto
            {
                Skills = payload.Skills ?? [],
                Requirements = payload.Requirements ?? [],
                Technologies = payload.Technologies ?? [],
                Experience = payload.Experience ?? string.Empty
            };
        }
        catch (JsonException)
        {
            return new JobDescriptionExtractionDto();
        }
    }

    private static string ExtractJsonObject(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            return string.Empty;
        }

        return text.Substring(start, end - start + 1);
    }

    private sealed class ExtractedPayload
    {
        public List<string>? Skills { get; set; }

        public List<string>? Requirements { get; set; }

        public List<string>? Technologies { get; set; }

        public string? Experience { get; set; }
    }
}
