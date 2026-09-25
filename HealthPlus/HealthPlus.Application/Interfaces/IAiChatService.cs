using HealthPlus.Application.DTOs.AiChat;

namespace HealthPlus.Application.Interfaces;

public interface IAiChatService
{
    Task<AiChatResponse> SendMessageAsync(Guid userId, SendAiMessageRequest request, CancellationToken ct = default);
    Task<IEnumerable<AiConversationSummaryResponse>> GetMyConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<AiMessageResponse>> GetMessagesAsync(Guid userId, Guid conversationId, CancellationToken ct = default);
}
