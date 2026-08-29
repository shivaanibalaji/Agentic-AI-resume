using System.Runtime.Serialization;

namespace Resume.Application.DTO.Chat;

/// <summary>
/// Represents a single chunk of a streamed chat response.
/// </summary>
[DataContract]
public sealed class ChatStreamChunkDto
{
    /// <summary>
    /// Gets or sets the incremental portion of the answer produced since the previous chunk.
    /// </summary>
    [DataMember]
    public string? AnswerPart { get; set; }

    /// <summary>
    /// Gets or sets the collection of source chunks used to answer the question.
    /// Present only on the terminal chunk.
    /// </summary>
    [DataMember]
    public IReadOnlyList<SourceDto>? Sources { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the terminal chunk of the stream.
    /// </summary>
    [DataMember]
    public bool Done { get; set; }
}