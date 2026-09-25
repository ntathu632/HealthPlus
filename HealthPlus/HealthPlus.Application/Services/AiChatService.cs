using HealthPlus.Application.DTOs.AiChat;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class AiChatService : IAiChatService
{
    private const string SystemPrompt =
        "Bạn là trợ lý AI về sức khỏe của ứng dụng Health+. Trả lời ngắn gọn, dễ hiểu bằng tiếng Việt, " +
        "tập trung vào các chủ đề sức khỏe, dinh dưỡng, triệu chứng thông thường và thói quen sống lành mạnh. " +
        "Luôn nhắc rằng đây chỉ là thông tin tham khảo, KHÔNG thay thế chẩn đoán hay tư vấn của bác sĩ, và khuyến khích " +
        "người dùng đặt lịch khám với bác sĩ trên Health+ nếu triệu chứng nghiêm trọng hoặc kéo dài. " +
        "Không đưa ra chẩn đoán chắc chắn hay kê đơn thuốc cụ thể.";

    private readonly IUnitOfWork _uow;
    private readonly IAiCompletionClient _aiClient;

    public AiChatService(IUnitOfWork uow, IAiCompletionClient aiClient)
    {
        _uow = uow;
        _aiClient = aiClient;
    }

    public async Task<AiChatResponse> SendMessageAsync(Guid userId, SendAiMessageRequest request, CancellationToken ct = default)
    {
        var content = request.Message.Trim();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Nội dung tin nhắn không được để trống.");

        AiConversation conversation;
        if (request.ConversationId.HasValue)
        {
            conversation = await _uow.AiConversations.GetByIdAsync(request.ConversationId.Value, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");
            if (conversation.UserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập cuộc trò chuyện này.");
        }
        else
        {
            conversation = new AiConversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = content.Length > 60 ? content[..60] + "…" : content,
            };
            await _uow.AiConversations.AddAsync(conversation, ct);
        }

        var userMessage = new AiMessage { Id = Guid.NewGuid(), ConversationId = conversation.Id, Role = "user", Content = content };
        await _uow.AiMessages.AddAsync(userMessage, ct);
        await _uow.SaveChangesAsync(ct);

        var history = (await _uow.AiMessages.FindAsync(m => m.ConversationId == conversation.Id, ct))
            .OrderBy(m => m.CreatedAt)
            .Select(m => (m.Role, m.Content))
            .ToList();

        var replyText = await _aiClient.GetCompletionAsync(SystemPrompt, history, ct);

        var assistantMessage = new AiMessage { Id = Guid.NewGuid(), ConversationId = conversation.Id, Role = "assistant", Content = replyText };
        await _uow.AiMessages.AddAsync(assistantMessage, ct);
        await _uow.SaveChangesAsync(ct);

        return new AiChatResponse
        {
            ConversationId = conversation.Id,
            Reply = Map(assistantMessage),
        };
    }

    public async Task<IEnumerable<AiConversationSummaryResponse>> GetMyConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        var conversations = await _uow.AiConversations.FindAsync(c => c.UserId == userId, ct);
        return conversations.OrderByDescending(c => c.CreatedAt).Select(c => new AiConversationSummaryResponse
        {
            Id = c.Id,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
        });
    }

    public async Task<IEnumerable<AiMessageResponse>> GetMessagesAsync(Guid userId, Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _uow.AiConversations.GetByIdAsync(conversationId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");
        if (conversation.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập cuộc trò chuyện này.");

        var messages = await _uow.AiMessages.FindAsync(m => m.ConversationId == conversationId, ct);
        return messages.OrderBy(m => m.CreatedAt).Select(Map);
    }

    private static AiMessageResponse Map(AiMessage m) => new()
    {
        Id = m.Id,
        Role = m.Role,
        Content = m.Content,
        CreatedAt = m.CreatedAt,
    };
}
