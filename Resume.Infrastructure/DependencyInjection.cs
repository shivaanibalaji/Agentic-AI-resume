using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;
using Resume.Application;
using Resume.Application.Interfaces;
using Resume.Infrastructure.AI.Ollama;
using Resume.Infrastructure.Knowledge;
using Resume.Infrastructure.Persistence;
using Resume.Infrastructure.Persistence.Repositories;

namespace Resume.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Supabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL connection string is missing. Set the SUPABASE_CONNECTION_STRING environment variable " +
                "or ConnectionStrings:Supabase in appsettings.json.");
        }

        services.AddDbContext<ResumeDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
        services.AddScoped<IVectorSearchRepository, VectorSearchRepository>();

        services.Configure<KnowledgeBaseOptions>(configuration.GetSection(KnowledgeBaseOptions.SectionName));
        services.AddSingleton<IMarkdownChunker, MarkdownChunker>();
        services.AddScoped<IMarkdownDocumentLoader, MarkdownDocumentLoader>();

        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((serviceProvider, httpClient) =>
            ConfigureOllamaHttpClient(serviceProvider, httpClient));
        services.AddHttpClient<ILlmService, OllamaLlmService>((serviceProvider, httpClient) =>
            ConfigureOllamaHttpClient(serviceProvider, httpClient));

        return services;
    }

    private static void ConfigureOllamaHttpClient(IServiceProvider serviceProvider, HttpClient httpClient)
    {
        var ollama = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
        httpClient.BaseAddress = new Uri(ollama.BaseUrl.TrimEnd('/') + "/");
        httpClient.Timeout = TimeSpan.FromMinutes(10);
    }
}
