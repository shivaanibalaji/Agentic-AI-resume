namespace Resume.Application.DTO.Knowledge;

public record IngestionResultDto(int TotalFiles, int NewDocuments, int UpdatedDocuments, int TotalChunks);
