using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resume.Domain.Entities;

namespace Resume.Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Document)
            .WithMany(d => d.Chunks)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Content)
            .IsRequired();

        builder.Property(c => c.Section)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        builder.Property(c => c.Embedding)
            .IsRequired()
            .HasColumnType("vector(1024)");

        builder.HasIndex(c => c.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");

        builder.Property(c => c.Metadata)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => c.DocumentId);
    }
}
