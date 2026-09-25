using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthPlus.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaccineScheduleTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaccineName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DoseNumber = table.Column<int>(type: "int", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: true),
                    RecommendedAge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccineScheduleTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChronicDiseases = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InsuranceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RemindAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Push"),
                    RepeatType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RepeatUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationSettings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    VaccineReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MedicineReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FollowUpReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    QuietHourStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    QuietHourEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    FcmToken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserNotificationSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    HealthRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    HeightCm = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SystolicBp = table.Column<int>(type: "int", nullable: true),
                    DiastolicBp = table.Column<int>(type: "int", nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    BloodSugar = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthMetrics_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Hospital = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Treatment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistory_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vaccines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaccineName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoseNumber = table.Column<int>(type: "int", nullable: false),
                    InjectionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdministeredBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SideEffects = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Completed"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaccines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vaccines_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vaccines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReminderLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReminderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMsg = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReminderLogs_Reminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "Reminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MedicalHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalDocuments_MedicalHistory_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalTable: "MedicalHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MedicalHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageStorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OcrRawText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcrConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_MedicalHistory_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalTable: "MedicalHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FrequencyPerDay = table.Column<int>(type: "int", nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    Timing = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { 1, "read", null, "users.read", "users" },
                    { 2, "write", null, "users.write", "users" },
                    { 3, "delete", null, "users.delete", "users" },
                    { 4, "read", null, "health_records.read", "health_records" },
                    { 5, "write", null, "health_records.write", "health_records" },
                    { 6, "read", null, "vaccines.read", "vaccines" },
                    { 7, "write", null, "vaccines.write", "vaccines" },
                    { 8, "read", null, "audit_logs.read", "audit_logs" },
                    { 9, "manage", null, "system.manage", "system" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5457), "Quản trị viên hệ thống", "Admin" },
                    { 2, new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5461), "Bác sĩ/nhân viên y tế", "Doctor" },
                    { 3, new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5463), "Người dùng thông thường", "User" }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "Description", "SettingKey", "SettingValue", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Tên ứng dụng", "app.name", "Health+", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5824), null },
                    { 2, "Phiên bản hiện tại", "app.version", "1.0.0", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5828), null },
                    { 3, "Thời hạn access token (phút)", "jwt.access_token_expiry", "15", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5831), null },
                    { 4, "Thời hạn refresh token (ngày)", "jwt.refresh_token_expiry", "7", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5832), null },
                    { 5, "Nhà cung cấp OCR", "ocr.provider", "tesseract", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5833), null },
                    { 6, "Nhà cung cấp storage", "storage.provider", "local", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5834), null },
                    { 7, "Số ngày nhắc trước lịch tiêm/tái khám", "reminder.advance_days", "3", new DateTime(2026, 6, 6, 2, 19, 19, 737, DateTimeKind.Utc).AddTicks(5836), null }
                });

            migrationBuilder.InsertData(
                table: "VaccineScheduleTemplates",
                columns: new[] { "Id", "DoseNumber", "IntervalDays", "Notes", "RecommendedAge", "VaccineName" },
                values: new object[,]
                {
                    { 1, 1, null, "Tiêm trong 24 giờ đầu", "Sơ sinh", "BCG (Lao)" },
                    { 2, 1, null, "Tiêm trong 24 giờ đầu", "Sơ sinh", "Viêm gan B" },
                    { 3, 2, 60, null, "2 tháng", "Viêm gan B" },
                    { 4, 3, 60, null, "4 tháng", "Viêm gan B" },
                    { 5, 1, null, null, "2 tháng", "Bạch hầu-Ho gà-Uốn ván (DTP)" },
                    { 6, 2, 30, null, "3 tháng", "Bạch hầu-Ho gà-Uốn ván (DTP)" },
                    { 7, 3, 30, null, "4 tháng", "Bạch hầu-Ho gà-Uốn ván (DTP)" },
                    { 8, 1, null, null, "2 tháng", "Bại liệt (OPV)" },
                    { 9, 2, 30, null, "3 tháng", "Bại liệt (OPV)" },
                    { 10, 3, 30, null, "4 tháng", "Bại liệt (OPV)" },
                    { 11, 1, null, null, "9 tháng", "Sởi" },
                    { 12, 1, 90, null, "12 tháng", "Sởi-Quai bị-Rubella" },
                    { 13, 1, null, null, "12 tháng", "Viêm não Nhật Bản B" },
                    { 14, 2, 14, "14 ngày sau mũi 1", "12 tháng", "Viêm não Nhật Bản B" },
                    { 15, 3, 365, "1 năm sau mũi 2", "24 tháng", "Viêm não Nhật Bản B" },
                    { 16, 1, null, null, "12 tuổi trở lên", "COVID-19" },
                    { 17, 2, 28, null, "12 tuổi trở lên", "COVID-19" },
                    { 18, 1, null, "Nhắc lại mỗi năm", "Hàng năm", "Cúm" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetrics_HealthRecordId",
                table: "HealthMetrics",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetrics_MeasuredAt",
                table: "HealthMetrics",
                column: "MeasuredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_UserId",
                table: "HealthRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalDocuments_MedicalHistoryId",
                table: "MedicalDocuments",
                column: "MedicalHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistory_HealthRecordId",
                table: "MedicalHistory",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistory_UserId",
                table: "MedicalHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistory_VisitDate",
                table: "MedicalHistory",
                column: "VisitDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalHistoryId",
                table: "Prescriptions",
                column: "MedicalHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UserId",
                table: "Prescriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderLogs_ReminderId",
                table: "ReminderLogs",
                column: "ReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_IsSent",
                table: "Reminders",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_RemindAt",
                table: "Reminders",
                column: "RemindAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ReminderType",
                table: "Reminders",
                column: "ReminderType");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_SettingKey",
                table: "SystemSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccines_HealthRecordId",
                table: "Vaccines",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccines_Status",
                table: "Vaccines",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccines_UserId",
                table: "Vaccines",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "HealthMetrics");

            migrationBuilder.DropTable(
                name: "MedicalDocuments");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ReminderLogs");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserNotificationSettings");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Vaccines");

            migrationBuilder.DropTable(
                name: "VaccineScheduleTemplates");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "MedicalHistory");

            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
