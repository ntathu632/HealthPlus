using System.Text;
using HealthPlus.Application;
using HealthPlus.Application.Interfaces;
using HealthPlus.Conventions;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;
using HealthPlus.Infrastructure;
using HealthPlus.Middleware;
using HealthPlus.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// ─── Services ────────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseControllerConvention()))
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpContextAccessor();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ─── Swagger with Bearer auth ─────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthPlus API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Pipeline ─────────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

    // Tài khoản demo cố định (admin/bacsi/benhnhan, mật khẩu "123") — chỉ seed 1 lần khi chưa tồn tại.
    if (!await uow.Users.AnyAsync(u => u.Email == "admin"))
    {
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin",
            PasswordHash = passwordService.Hash("123"),
            FullName = "Quản trị viên",
            IsActive = true,
            IsEmailVerified = true,
        };
        var bacsi = new User
        {
            Id = Guid.NewGuid(),
            Email = "bacsi",
            PasswordHash = passwordService.Hash("123"),
            FullName = "Bác sĩ Demo",
            IsActive = true,
            IsEmailVerified = true,
        };
        var benhnhan = new User
        {
            Id = Guid.NewGuid(),
            Email = "benhnhan",
            PasswordHash = passwordService.Hash("123"),
            FullName = "Bệnh nhân Demo",
            IsActive = true,
            IsEmailVerified = true,
        };

        await uow.Users.AddAsync(admin);
        await uow.Users.AddAsync(bacsi);
        await uow.Users.AddAsync(benhnhan);

        await uow.UserRoles.AddAsync(new UserRole { UserId = admin.Id, RoleId = 1 });
        await uow.UserRoles.AddAsync(new UserRole { UserId = bacsi.Id, RoleId = 2 });
        await uow.UserRoles.AddAsync(new UserRole { UserId = benhnhan.Id, RoleId = 3 });

        await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = admin.Id });
        await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = bacsi.Id });
        await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = benhnhan.Id });

        await uow.DoctorPatients.AddAsync(new DoctorPatient
        {
            Id = Guid.NewGuid(),
            DoctorId = bacsi.Id,
            PatientId = benhnhan.Id,
            AssignedBy = admin.Id,
            IsActive = true,
        });

        await uow.HealthRecords.AddAsync(new HealthRecord
        {
            Id = Guid.NewGuid(),
            UserId = benhnhan.Id,
            ProfileName = "Bản thân",
            FullName = benhnhan.FullName,
            DateOfBirth = new DateOnly(1995, 1, 1),
            Gender = Gender.Other,
        });

        await uow.SaveChangesAsync();

        Log.Information("Đã seed tài khoản demo: admin/123 (Admin), bacsi/123 (Bác sĩ), benhnhan/123 (Bệnh nhân)");
    }

    // Dữ liệu mẫu phong phú hơn cho demo (thêm bác sĩ/bệnh nhân/lịch sử khám/vaccine/đơn thuốc/lịch hẹn/nhắc nhở đa dạng)
    // — chạy độc lập với khối seed 3 tài khoản gốc ở trên, tự kiểm tra đã seed chưa qua "bacsi2".
    if (!await uow.Users.AnyAsync(u => u.Email == "bacsi2"))
    {
        var admin = await uow.Users.FirstOrDefaultAsync(u => u.Email == "admin");
        var bacsi = await uow.Users.FirstOrDefaultAsync(u => u.Email == "bacsi");
        var benhnhan = await uow.Users.FirstOrDefaultAsync(u => u.Email == "benhnhan");

        if (admin != null && bacsi != null && benhnhan != null)
        {
            var bacsi2 = new User
            {
                Id = Guid.NewGuid(), Email = "bacsi2", PasswordHash = passwordService.Hash("123"),
                FullName = "BS. Trần Thị Hương", PhoneNumber = "0912345678", IsActive = true, IsEmailVerified = true,
            };
            var bacsi3 = new User
            {
                Id = Guid.NewGuid(), Email = "bacsi3", PasswordHash = passwordService.Hash("123"),
                FullName = "BS. Lê Văn Minh", PhoneNumber = "0923456789", IsActive = true, IsEmailVerified = true,
            };
            var benhnhan2 = new User
            {
                Id = Guid.NewGuid(), Email = "benhnhan2", PasswordHash = passwordService.Hash("123"),
                FullName = "Nguyễn Thị Lan", PhoneNumber = "0934567890", IsActive = true, IsEmailVerified = true,
            };
            await uow.Users.AddAsync(bacsi2);
            await uow.Users.AddAsync(bacsi3);
            await uow.Users.AddAsync(benhnhan2);

            await uow.UserRoles.AddAsync(new UserRole { UserId = bacsi2.Id, RoleId = 2 });
            await uow.UserRoles.AddAsync(new UserRole { UserId = bacsi3.Id, RoleId = 2 });
            await uow.UserRoles.AddAsync(new UserRole { UserId = benhnhan2.Id, RoleId = 3 });
            await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = bacsi2.Id });
            await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = bacsi3.Id });
            await uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = benhnhan2.Id });

            // benhnhan có thêm 1 hồ sơ cho con, và được gán thêm bác sĩ thứ 2 (đa bác sĩ)
            var childRecord = new HealthRecord
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, ProfileName = "Con trai", FullName = "Nguyễn Bảo Nam",
                DateOfBirth = new DateOnly(2019, 3, 15), Gender = Gender.Male, BloodType = BloodType.OPositive,
                Allergies = "Dị ứng hải sản",
            };
            await uow.HealthRecords.AddAsync(childRecord);

            var selfRecord = await uow.HealthRecords.FirstOrDefaultAsync(h => h.UserId == benhnhan.Id && h.ProfileName == "Bản thân")
                ?? childRecord;

            await uow.DoctorPatients.AddAsync(new DoctorPatient
            {
                Id = Guid.NewGuid(), DoctorId = bacsi2.Id, PatientId = benhnhan.Id, AssignedBy = admin.Id, IsActive = true,
            });
            await uow.DoctorPatients.AddAsync(new DoctorPatient
            {
                Id = Guid.NewGuid(), DoctorId = bacsi2.Id, PatientId = benhnhan2.Id, AssignedBy = admin.Id, IsActive = true,
            });

            var patient2Record = new HealthRecord
            {
                Id = Guid.NewGuid(), UserId = benhnhan2.Id, ProfileName = "Bản thân", FullName = benhnhan2.FullName,
                DateOfBirth = new DateOnly(1990, 6, 20), Gender = Gender.Female, BloodType = BloodType.APositive,
                ChronicDiseases = "Cao huyết áp",
            };
            await uow.HealthRecords.AddAsync(patient2Record);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Lịch sử khám đa dạng hơn cho benhnhan
            await uow.MedicalHistories.AddAsync(new MedicalHistory
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, HealthRecordId = selfRecord.Id,
                VisitDate = today.AddDays(-30), Hospital = "BV Đại học Y Dược", Specialty = "Tiêu hóa",
                DoctorName = "BS. Trần Thị Hương", Diagnosis = "Viêm dạ dày nhẹ",
                Treatment = "Thuốc kháng acid, ăn uống điều độ", Notes = "Tái khám sau 2 tuần nếu còn đau",
            });
            await uow.MedicalHistories.AddAsync(new MedicalHistory
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, HealthRecordId = childRecord.Id,
                VisitDate = today.AddDays(-10), Hospital = "BV Nhi Đồng 1", Specialty = "Nhi khoa",
                DoctorName = "BS. Trần Thị Hương", Diagnosis = "Viêm họng cấp",
                Treatment = "Kháng sinh 5 ngày, súc miệng nước muối", FollowUpDate = today.AddDays(4),
            });

            // Vaccine đa dạng trạng thái: đã tiêm, sắp tới hạn, quá hạn
            await uow.Vaccines.AddAsync(new Vaccine
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, HealthRecordId = childRecord.Id,
                VaccineName = "Bạch hầu-Ho gà-Uốn ván (DTP)", DoseNumber = 1, InjectionDate = today.AddMonths(-6),
                Location = "Trung tâm y tế dự phòng Q1", AdministeredBy = "BS. Trần Thị Hương",
                Status = VaccineStatus.Completed,
            });
            await uow.Vaccines.AddAsync(new Vaccine
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, HealthRecordId = childRecord.Id,
                VaccineName = "Bạch hầu-Ho gà-Uốn ván (DTP)", DoseNumber = 2, InjectionDate = today.AddMonths(-5),
                NextDueDate = today.AddDays(10), Location = "Trung tâm y tế dự phòng Q1",
                Status = VaccineStatus.Scheduled,
            });
            await uow.Vaccines.AddAsync(new Vaccine
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, HealthRecordId = selfRecord.Id,
                VaccineName = "Cúm", DoseNumber = 1, InjectionDate = today.AddYears(-1),
                NextDueDate = today.AddDays(-15), Location = "Phòng khám Family Medical Practice",
                Status = VaccineStatus.Overdue, Notes = "Nên tiêm nhắc lại hàng năm",
            });

            // Đơn thuốc thứ 2 với nhiều thuốc, đã hoàn thành
            var rx2 = new Prescription
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, Status = PrescriptionStatus.Completed,
                ProcessedAt = DateTime.UtcNow.AddDays(-9),
            };
            await uow.Prescriptions.AddAsync(rx2);
            await uow.PrescriptionItems.AddAsync(new PrescriptionItem
            {
                Id = Guid.NewGuid(), PrescriptionId = rx2.Id, MedicineName = "Amoxicillin", Dosage = "500mg",
                FrequencyPerDay = 2, DurationDays = 5, Timing = "Sau ăn", IsConfirmed = true,
            });
            await uow.PrescriptionItems.AddAsync(new PrescriptionItem
            {
                Id = Guid.NewGuid(), PrescriptionId = rx2.Id, MedicineName = "Nước muối súc miệng", Dosage = "1 chai",
                Instructions = "Súc miệng 3 lần/ngày", IsConfirmed = true,
            });

            // Lịch hẹn đa dạng trạng thái với nhiều bác sĩ khác nhau
            await uow.Appointments.AddAsync(new Appointment
            {
                Id = Guid.NewGuid(), DoctorId = bacsi2.Id, PatientId = benhnhan.Id,
                AppointmentTime = DateTime.UtcNow.AddDays(3), Reason = "Tái khám dạ dày",
                Status = AppointmentStatus.Confirmed, Notes = "Đã xác nhận, nhớ nhịn ăn sáng trước khi khám",
            });
            await uow.Appointments.AddAsync(new Appointment
            {
                Id = Guid.NewGuid(), DoctorId = bacsi3.Id, PatientId = benhnhan.Id,
                AppointmentTime = DateTime.UtcNow.AddDays(7), Reason = "Khám tim mạch định kỳ",
                Status = AppointmentStatus.Pending,
            });
            await uow.Appointments.AddAsync(new Appointment
            {
                Id = Guid.NewGuid(), DoctorId = bacsi.Id, PatientId = benhnhan.Id,
                AppointmentTime = DateTime.UtcNow.AddDays(-5), Reason = "Khám tổng quát",
                Status = AppointmentStatus.Cancelled,
            });

            // Nhắc nhở đa dạng loại
            await uow.Reminders.AddAsync(new Reminder
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, ReminderType = ReminderType.Medicine,
                Title = "Uống thuốc Amoxicillin", Message = "Đến giờ uống thuốc sau ăn trưa",
                RemindAt = DateTime.UtcNow.AddHours(3), Channel = ReminderChannel.Push,
            });
            await uow.Reminders.AddAsync(new Reminder
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, ReminderType = ReminderType.Vaccine,
                Title = "Tiêm nhắc lại vaccine Cúm", Message = "Vaccine Cúm đã quá hạn tiêm nhắc lại",
                RemindAt = DateTime.UtcNow.AddDays(1), Channel = ReminderChannel.Email,
            });
            await uow.Reminders.AddAsync(new Reminder
            {
                Id = Guid.NewGuid(), UserId = benhnhan.Id, ReminderType = ReminderType.FollowUp,
                Title = "Tái khám viêm họng cho con", Message = "Lịch tái khám tại BV Nhi Đồng 1",
                RemindAt = DateTime.UtcNow.AddDays(4), Channel = ReminderChannel.Push,
            });

            await uow.SaveChangesAsync();

            Log.Information("Đã seed thêm dữ liệu mẫu đa dạng: 2 bác sĩ (bacsi2, bacsi3), 1 bệnh nhân (benhnhan2), " +
                "thêm hồ sơ/lịch sử khám/vaccine/đơn thuốc/lịch hẹn/nhắc nhở cho benhnhan");
        }
    }

    // Bệnh viện (tên hư cấu, không phải tên thật) + gán chuyên khoa/phí tư vấn cho các bác sĩ demo
    // — dùng cho tính năng "tư vấn bác sĩ online trả phí". Chạy độc lập, tự kiểm tra qua bảng Hospitals.
    if (await uow.Hospitals.CountAsync() == 0)
    {
        var thanhDo = new Hospital { Id = Guid.NewGuid(), Name = "Bệnh viện Đa khoa Quốc tế Thành Đô", Address = "12 Đường Sức Khỏe, Quận 1, TP.HCM" };
        var hongPhuc = new Hospital { Id = Guid.NewGuid(), Name = "Bệnh viện Đa khoa Hồng Phúc", Address = "45 Đường Hòa Bình, Quận 3, TP.HCM" };
        var vietTam = new Hospital { Id = Guid.NewGuid(), Name = "Bệnh viện Đa khoa Việt Tâm", Address = "78 Đường Nguyễn Trãi, Quận 5, TP.HCM" };
        var songHan = new Hospital { Id = Guid.NewGuid(), Name = "Bệnh viện Quốc tế Sông Hàn", Address = "23 Đường Bạch Đằng, Đà Nẵng" };

        await uow.Hospitals.AddAsync(thanhDo);
        await uow.Hospitals.AddAsync(hongPhuc);
        await uow.Hospitals.AddAsync(vietTam);
        await uow.Hospitals.AddAsync(songHan);

        var bacsi = await uow.Users.FirstOrDefaultAsync(u => u.Email == "bacsi");
        var bacsi2 = await uow.Users.FirstOrDefaultAsync(u => u.Email == "bacsi2");
        var bacsi3 = await uow.Users.FirstOrDefaultAsync(u => u.Email == "bacsi3");

        if (bacsi != null)
        {
            bacsi.HospitalId = thanhDo.Id;
            bacsi.Specialty = "Nội khoa";
            bacsi.ConsultationFee = 150_000m;
            uow.Users.Update(bacsi);
        }
        if (bacsi2 != null)
        {
            bacsi2.HospitalId = hongPhuc.Id;
            bacsi2.Specialty = "Tiêu hoá";
            bacsi2.ConsultationFee = 250_000m;
            uow.Users.Update(bacsi2);
        }
        if (bacsi3 != null)
        {
            bacsi3.HospitalId = vietTam.Id;
            bacsi3.Specialty = "Tim mạch";
            bacsi3.ConsultationFee = 300_000m;
            uow.Users.Update(bacsi3);
        }

        await uow.SaveChangesAsync();
        Log.Information("Đã seed 4 bệnh viện (tên hư cấu) và gán chuyên khoa/phí tư vấn cho bác sĩ demo");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
