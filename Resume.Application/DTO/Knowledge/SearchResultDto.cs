namespace Resume.Application.DTO.Knowledge;

public record SearchResultDto(string Document, string Section, int ChunkIndex, string Content, double Score);
