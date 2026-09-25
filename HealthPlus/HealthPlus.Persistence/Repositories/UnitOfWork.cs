using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(context);
        Roles = new GenericRepository<Role>(context);
        Permissions = new GenericRepository<Permission>(context);
        UserRoles = new GenericRepository<UserRole>(context);
        RolePermissions = new GenericRepository<RolePermission>(context);
        RefreshTokens = new GenericRepository<RefreshToken>(context);
        HealthRecords = new GenericRepository<HealthRecord>(context);
        HealthMetrics = new GenericRepository<HealthMetric>(context);
        MedicalHistories = new GenericRepository<MedicalHistory>(context);
        MedicalDocuments = new GenericRepository<MedicalDocument>(context);
        Prescriptions = new GenericRepository<Prescription>(context);
        PrescriptionItems = new GenericRepository<PrescriptionItem>(context);
        Vaccines = new GenericRepository<Vaccine>(context);
        VaccineScheduleTemplates = new GenericRepository<VaccineScheduleTemplate>(context);
        Reminders = new GenericRepository<Reminder>(context);
        ReminderLogs = new GenericRepository<ReminderLog>(context);
        UserNotificationSettings = new GenericRepository<UserNotificationSetting>(context);
        AuditLogs = new GenericRepository<AuditLog>(context);
        SystemSettings = new GenericRepository<SystemSetting>(context);
        DoctorPatients = new GenericRepository<DoctorPatient>(context);
        Appointments = new GenericRepository<Appointment>(context);
        Hospitals = new GenericRepository<Hospital>(context);
        Payments = new GenericRepository<Payment>(context);
        AiConversations = new GenericRepository<AiConversation>(context);
        AiMessages = new GenericRepository<AiMessage>(context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Role> Roles { get; }
    public IGenericRepository<Permission> Permissions { get; }
    public IGenericRepository<UserRole> UserRoles { get; }
    public IGenericRepository<RolePermission> RolePermissions { get; }
    public IGenericRepository<RefreshToken> RefreshTokens { get; }
    public IGenericRepository<HealthRecord> HealthRecords { get; }
    public IGenericRepository<HealthMetric> HealthMetrics { get; }
    public IGenericRepository<MedicalHistory> MedicalHistories { get; }
    public IGenericRepository<MedicalDocument> MedicalDocuments { get; }
    public IGenericRepository<Prescription> Prescriptions { get; }
    public IGenericRepository<PrescriptionItem> PrescriptionItems { get; }
    public IGenericRepository<Vaccine> Vaccines { get; }
    public IGenericRepository<VaccineScheduleTemplate> VaccineScheduleTemplates { get; }
    public IGenericRepository<Reminder> Reminders { get; }
    public IGenericRepository<ReminderLog> ReminderLogs { get; }
    public IGenericRepository<UserNotificationSetting> UserNotificationSettings { get; }
    public IGenericRepository<AuditLog> AuditLogs { get; }
    public IGenericRepository<SystemSetting> SystemSettings { get; }
    public IGenericRepository<DoctorPatient> DoctorPatients { get; }
    public IGenericRepository<Appointment> Appointments { get; }
    public IGenericRepository<Hospital> Hospitals { get; }
    public IGenericRepository<Payment> Payments { get; }
    public IGenericRepository<AiConversation> AiConversations { get; }
    public IGenericRepository<AiMessage> AiMessages { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}
