using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace Resume.Infrastructure.Persistence;

/// <summary>
/// Creates <see cref="ResumeDbContext"/> instances for design-time tooling such as EF Core migrations.
/// </summary>
public class ResumeDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ResumeDbContext>
{
    /// <summary>
    /// Creates a <see cref="ResumeDbContext"/> using the configured connection string.
    /// </summary>
    /// <param name="args">The design-time arguments.</param>
    /// <returns>A configured <see cref="ResumeDbContext"/>.</returns>
    public ResumeDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=resume_design_time;Username=postgres;Password=postgres";

        DbContextOptionsBuilder<ResumeDbContext> optionsBuilder = new DbContextOptionsBuilder<ResumeDbContext>();

        optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.UseVector());

        return new ResumeDbContext(optionsBuilder.Options);
    }
}
