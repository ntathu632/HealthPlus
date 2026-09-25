using System.Linq.Expressions;
using HealthPlus.Application.DTOs.AiChat;
using HealthPlus.Application.Interfaces;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class AiChatServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<AiConversation>> _conversations = new();
    private readonly Mock<IGenericRepository<AiMessage>> _messages = new();
    private readonly Mock<IAiCompletionClient> _aiClient = new();
    private readonly AiChatService _sut;

    private readonly Guid _userId = Guid.NewGuid();

    public AiChatServiceTests()
    {
        _uow.SetupGet(u => u.AiConversations).Returns(_conversations.Object);
        _uow.SetupGet(u => u.AiMessages).Returns(_messages.Object);
        _sut = new AiChatService(_uow.Object, _aiClient.Object);
    }

    [Fact]
    public async Task SendMessageAsync_BlankMessage_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SendMessageAsync(_userId, new SendAiMessageRequest { Message = "   " }, CancellationToken.None));

        _aiClient.Verify(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string)>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_NoConversationId_CreatesNewConversationWithTruncatedTitle()
    {
        var longMessage = new string('a', 80);
        AiConversation? created = null;
        _conversations.Setup(c => c.AddAsync(It.IsAny<AiConversation>(), It.IsAny<CancellationToken>()))
            .Callback<AiConversation, CancellationToken>((c, _) => created = c)
            .Returns(Task.CompletedTask);
        _messages.Setup(m => m.FindAsync(It.IsAny<Expression<Func<AiMessage, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _aiClient.Setup(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Trả lời từ AI");

        var result = await _sut.SendMessageAsync(_userId, new SendAiMessageRequest { Message = longMessage }, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(_userId, created!.UserId);
        Assert.Equal(longMessage[..60] + "…", created.Title);
        Assert.Equal("Trả lời từ AI", result.Reply.Content);
        Assert.Equal("assistant", result.Reply.Role);
    }

    [Fact]
    public async Task SendMessageAsync_ExistingConversationBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        var conversationId = Guid.NewGuid();
        _conversations.Setup(c => c.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiConversation { Id = conversationId, UserId = Guid.NewGuid() });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.SendMessageAsync(_userId, new SendAiMessageRequest { ConversationId = conversationId, Message = "Hi" }, CancellationToken.None));

        _aiClient.Verify(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<(string, string)>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_ExistingConversationNotFound_ThrowsKeyNotFoundException()
    {
        var conversationId = Guid.NewGuid();
        _conversations.Setup(c => c.GetByIdAsync(conversationId, It.IsAny<CancellationToken>())).ReturnsAsync((AiConversation?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.SendMessageAsync(_userId, new SendAiMessageRequest { ConversationId = conversationId, Message = "Hi" }, CancellationToken.None));
    }

    [Fact]
    public async Task GetMessagesAsync_ConversationBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        var conversationId = Guid.NewGuid();
        _conversations.Setup(c => c.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiConversation { Id = conversationId, UserId = Guid.NewGuid() });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.GetMessagesAsync(_userId, conversationId, CancellationToken.None));
    }

    [Fact]
    public async Task GetMyConversationsAsync_ReturnsConversationsOrderedByMostRecentFirst()
    {
        var older = new AiConversation { Id = Guid.NewGuid(), UserId = _userId, CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var newer = new AiConversation { Id = Guid.NewGuid(), UserId = _userId, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        _conversations.Setup(c => c.FindAsync(It.IsAny<Expression<Func<AiConversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([older, newer]);

        var result = (await _sut.GetMyConversationsAsync(_userId, CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }
}
