using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class AiMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;

    public AiConversation Conversation { get; set; } = null!;
}
