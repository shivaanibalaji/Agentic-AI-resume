using System.Runtime.Serialization;

namespace Resume.Application.DTO.Chat;

/// <summary>
/// Represents a chat response containing the generated answer and its supporting sources.
/// </summary>
[DataContract]
public sealed class ChatResponseDto
{
    /// <summary>
    /// Gets or sets the generated answer to the user's question.
    /// </summary>
    [DataMember]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of source chunks used to answer the question.
    /// </summary>
    [DataMember]
    public IReadOnlyList<SourceDto> Sources { get; set; } = [];
}
