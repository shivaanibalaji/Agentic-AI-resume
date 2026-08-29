using System.Runtime.Serialization;

namespace Resume.Application.DTO.Chat;

/// <summary>
/// Represents a chat request containing the user's message.
/// </summary>
[DataContract]
public sealed class ChatRequestDto
{
    /// <summary>
    /// Gets or sets the message sent by the user.
    /// </summary>
    [DataMember]
    public string Message { get; set; } = string.Empty;
}
