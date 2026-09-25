using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class DoctorPatient : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AssignedBy { get; set; }
    public bool IsActive { get; set; } = true;

    public User Doctor { get; set; } = null!;
    public User Patient { get; set; } = null!;
}
