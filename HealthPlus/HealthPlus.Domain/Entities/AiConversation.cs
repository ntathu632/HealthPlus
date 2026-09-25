using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }

    public User User { get; set; } = null!;
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}
