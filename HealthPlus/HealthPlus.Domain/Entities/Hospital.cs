using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class Hospital : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}
