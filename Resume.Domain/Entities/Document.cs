namespace Resume.Domain.Entities;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
