namespace Resume.Application.DTO.Chat;

public record ChatResponseDto(string Answer, IReadOnlyList<SourceDto> Sources);
