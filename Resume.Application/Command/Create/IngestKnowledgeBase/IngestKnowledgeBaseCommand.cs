using MediatR;
using Resume.Application.DTO.Knowledge;

namespace Resume.Application.Command.Create.IngestKnowledgeBase;

public record IngestKnowledgeBaseCommand : IRequest<IngestionResultDto>;
