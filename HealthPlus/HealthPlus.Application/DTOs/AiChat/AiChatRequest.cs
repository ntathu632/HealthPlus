namespace HealthPlus.Application.DTOs.AiChat;

public class SendAiMessageRequest
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}
