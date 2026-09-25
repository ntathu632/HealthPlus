using HealthPlus.Domain.Entities;

namespace HealthPlus.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<Permission> Permissions { get; }
    IGenericRepository<UserRole> UserRoles { get; }
    IGenericRepository<RolePermission> RolePermissions { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<HealthRecord> HealthRecords { get; }
    IGenericRepository<HealthMetric> HealthMetrics { get; }
    IGenericRepository<MedicalHistory> MedicalHistories { get; }
    IGenericRepository<MedicalDocument> MedicalDocuments { get; }
    IGenericRepository<Prescription> Prescriptions { get; }
    IGenericRepository<PrescriptionItem> PrescriptionItems { get; }
    IGenericRepository<Vaccine> Vaccines { get; }
    IGenericRepository<VaccineScheduleTemplate> VaccineScheduleTemplates { get; }
    IGenericRepository<Reminder> Reminders { get; }
    IGenericRepository<ReminderLog> ReminderLogs { get; }
    IGenericRepository<UserNotificationSetting> UserNotificationSettings { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }
    IGenericRepository<SystemSetting> SystemSettings { get; }
    IGenericRepository<DoctorPatient> DoctorPatients { get; }
    IGenericRepository<Appointment> Appointments { get; }
    IGenericRepository<Hospital> Hospitals { get; }
    IGenericRepository<Payment> Payments { get; }
    IGenericRepository<AiConversation> AiConversations { get; }
    IGenericRepository<AiMessage> AiMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
