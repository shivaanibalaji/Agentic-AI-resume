using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;
using Resume.Application;
using Resume.Application.Interfaces.IRepository;
using Resume.Application.Interfaces.IService;
using Resume.Infrastructure.AI.Ollama;
using Resume.Infrastructure.Persistence;
using Resume.Infrastructure.Repository;
using Resume.Infrastructure.Service;

namespace Resume.Infrastructure;

/// <summary>
/// Provides dependency injection extension methods for the infrastructure layer.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Registers the infrastructure layer services, including the database context,
    /// repositories, services, and configuration options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
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
        services.AddSingleton<IMarkdownChunkerService, MarkdownChunkerService>();
        services.AddScoped<IMarkdownDocumentLoaderService, MarkdownDocumentLoaderService>();

        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((serviceProvider, httpClient) =>
            ConfigureOllamaHttpClient(serviceProvider, httpClient));
        services.AddHttpClient<ILlmService, OllamaLlmService>((serviceProvider, httpClient) =>
            ConfigureOllamaHttpClient(serviceProvider, httpClient));

        return services;
    }

    private static void ConfigureOllamaHttpClient(IServiceProvider serviceProvider, HttpClient httpClient)
    {
        OllamaOptions ollama = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
        httpClient.BaseAddress = new Uri(ollama.BaseUrl.TrimEnd('/') + "/");
        httpClient.Timeout = TimeSpan.FromMinutes(10);
    }
}
