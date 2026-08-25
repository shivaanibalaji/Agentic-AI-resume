using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace Resume.Infrastructure.Persistence;

public class ResumeDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ResumeDbContext>
{
    public ResumeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=resume_design_time;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ResumeDbContext>();

        optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.UseVector());

        return new ResumeDbContext(optionsBuilder.Options);
    }
}
