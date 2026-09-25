using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthPlus.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Vaccine> Vaccines => Set<Vaccine>();
    public DbSet<VaccineScheduleTemplate> VaccineScheduleTemplates => Set<VaccineScheduleTemplate>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();
    public DbSet<UserNotificationSetting> UserNotificationSettings => Set<UserNotificationSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<DoctorPatient> DoctorPatients => Set<DoctorPatient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin",  Description = "Quản trị viên hệ thống" },
            new Role { Id = 2, Name = "Doctor", Description = "Bác sĩ/nhân viên y tế" },
            new Role { Id = 3, Name = "User",   Description = "Người dùng thông thường" }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1,  Name = "users.read",          Resource = "users",          Action = "read" },
            new Permission { Id = 2,  Name = "users.write",         Resource = "users",          Action = "write" },
            new Permission { Id = 3,  Name = "users.delete",        Resource = "users",          Action = "delete" },
            new Permission { Id = 4,  Name = "health_records.read", Resource = "health_records", Action = "read" },
            new Permission { Id = 5,  Name = "health_records.write",Resource = "health_records", Action = "write" },
            new Permission { Id = 6,  Name = "vaccines.read",       Resource = "vaccines",       Action = "read" },
            new Permission { Id = 7,  Name = "vaccines.write",      Resource = "vaccines",       Action = "write" },
            new Permission { Id = 8,  Name = "audit_logs.read",     Resource = "audit_logs",     Action = "read" },
            new Permission { Id = 9,  Name = "system.manage",       Resource = "system",         Action = "manage" }
        );

        // Admin gets all permissions
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = 1, PermissionId = 1 },
            new RolePermission { RoleId = 1, PermissionId = 2 },
            new RolePermission { RoleId = 1, PermissionId = 3 },
            new RolePermission { RoleId = 1, PermissionId = 4 },
            new RolePermission { RoleId = 1, PermissionId = 5 },
            new RolePermission { RoleId = 1, PermissionId = 6 },
            new RolePermission { RoleId = 1, PermissionId = 7 },
            new RolePermission { RoleId = 1, PermissionId = 8 },
            new RolePermission { RoleId = 1, PermissionId = 9 },
            // Doctor: chỉ đọc/ghi hồ sơ sức khỏe + vaccine của bệnh nhân được gán, không quản trị hệ thống
            new RolePermission { RoleId = 2, PermissionId = 4 },
            new RolePermission { RoleId = 2, PermissionId = 5 },
            new RolePermission { RoleId = 2, PermissionId = 6 },
            new RolePermission { RoleId = 2, PermissionId = 7 }
        );

        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting { Id = 1, SettingKey = "app.name",                SettingValue = "Health+",   Description = "Tên ứng dụng" },
            new SystemSetting { Id = 2, SettingKey = "app.version",             SettingValue = "1.0.0",     Description = "Phiên bản hiện tại" },
            new SystemSetting { Id = 3, SettingKey = "jwt.access_token_expiry", SettingValue = "15",        Description = "Thời hạn access token (phút)" },
            new SystemSetting { Id = 4, SettingKey = "jwt.refresh_token_expiry",SettingValue = "7",         Description = "Thời hạn refresh token (ngày)" },
            new SystemSetting { Id = 5, SettingKey = "ocr.provider",            SettingValue = "tesseract", Description = "Nhà cung cấp OCR" },
            new SystemSetting { Id = 6, SettingKey = "storage.provider",        SettingValue = "local",     Description = "Nhà cung cấp storage" },
            new SystemSetting { Id = 7, SettingKey = "reminder.advance_days",   SettingValue = "3",         Description = "Số ngày nhắc trước lịch tiêm/tái khám" }
        );

        modelBuilder.Entity<VaccineScheduleTemplate>().HasData(
            new VaccineScheduleTemplate { Id = 1,  VaccineName = "BCG (Lao)",                       DoseNumber = 1, IntervalDays = null, RecommendedAge = "Sơ sinh",           Notes = "Tiêm trong 24 giờ đầu" },
            new VaccineScheduleTemplate { Id = 2,  VaccineName = "Viêm gan B",                      DoseNumber = 1, IntervalDays = null, RecommendedAge = "Sơ sinh",           Notes = "Tiêm trong 24 giờ đầu" },
            new VaccineScheduleTemplate { Id = 3,  VaccineName = "Viêm gan B",                      DoseNumber = 2, IntervalDays = 60,   RecommendedAge = "2 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 4,  VaccineName = "Viêm gan B",                      DoseNumber = 3, IntervalDays = 60,   RecommendedAge = "4 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 5,  VaccineName = "Bạch hầu-Ho gà-Uốn ván (DTP)",   DoseNumber = 1, IntervalDays = null, RecommendedAge = "2 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 6,  VaccineName = "Bạch hầu-Ho gà-Uốn ván (DTP)",   DoseNumber = 2, IntervalDays = 30,   RecommendedAge = "3 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 7,  VaccineName = "Bạch hầu-Ho gà-Uốn ván (DTP)",   DoseNumber = 3, IntervalDays = 30,   RecommendedAge = "4 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 8,  VaccineName = "Bại liệt (OPV)",                  DoseNumber = 1, IntervalDays = null, RecommendedAge = "2 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 9,  VaccineName = "Bại liệt (OPV)",                  DoseNumber = 2, IntervalDays = 30,   RecommendedAge = "3 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 10, VaccineName = "Bại liệt (OPV)",                  DoseNumber = 3, IntervalDays = 30,   RecommendedAge = "4 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 11, VaccineName = "Sởi",                             DoseNumber = 1, IntervalDays = null, RecommendedAge = "9 tháng",           Notes = null },
            new VaccineScheduleTemplate { Id = 12, VaccineName = "Sởi-Quai bị-Rubella",             DoseNumber = 1, IntervalDays = 90,   RecommendedAge = "12 tháng",          Notes = null },
            new VaccineScheduleTemplate { Id = 13, VaccineName = "Viêm não Nhật Bản B",             DoseNumber = 1, IntervalDays = null, RecommendedAge = "12 tháng",          Notes = null },
            new VaccineScheduleTemplate { Id = 14, VaccineName = "Viêm não Nhật Bản B",             DoseNumber = 2, IntervalDays = 14,   RecommendedAge = "12 tháng",          Notes = "14 ngày sau mũi 1" },
            new VaccineScheduleTemplate { Id = 15, VaccineName = "Viêm não Nhật Bản B",             DoseNumber = 3, IntervalDays = 365,  RecommendedAge = "24 tháng",          Notes = "1 năm sau mũi 2" },
            new VaccineScheduleTemplate { Id = 16, VaccineName = "COVID-19",                        DoseNumber = 1, IntervalDays = null, RecommendedAge = "12 tuổi trở lên",   Notes = null },
            new VaccineScheduleTemplate { Id = 17, VaccineName = "COVID-19",                        DoseNumber = 2, IntervalDays = 28,   RecommendedAge = "12 tuổi trở lên",   Notes = null },
            new VaccineScheduleTemplate { Id = 18, VaccineName = "Cúm",                             DoseNumber = 1, IntervalDays = null, RecommendedAge = "Hàng năm",          Notes = "Nhắc lại mỗi năm" }
        );
    }
}
