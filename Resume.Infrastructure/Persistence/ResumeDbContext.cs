using Microsoft.EntityFrameworkCore;
using Resume.Domain.Entities;

namespace Resume.Infrastructure.Persistence;

public class ResumeDbContext(DbContextOptions<ResumeDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResumeDbContext).Assembly);
    }
}
