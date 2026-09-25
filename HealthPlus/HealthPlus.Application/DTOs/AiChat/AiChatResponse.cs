namespace HealthPlus.Application.DTOs.AiChat;

public class AiMessageResponse
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AiChatResponse
{
    public Guid ConversationId { get; set; }
    public AiMessageResponse Reply { get; set; } = null!;
}

public class AiConversationSummaryResponse
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
}
