namespace HealthPlus.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
