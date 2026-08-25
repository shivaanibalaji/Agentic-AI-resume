using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resume.Domain.Entities;

namespace Resume.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.HasIndex(d => d.FileName)
            .IsUnique();

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();
    }
}
