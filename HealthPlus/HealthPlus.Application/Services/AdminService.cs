using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Admin;
using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _password;

    public AdminService(IUnitOfWork uow, IPasswordService password)
    {
        _uow = uow;
        _password = password;
    }

    public async Task<PagedResult<AdminUserResponse>> GetUsersAsync(int page, int pageSize, string? search, int? roleId, CancellationToken ct = default)
    {
        var users = (await _uow.Users.GetAllAsync(ct)).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            users = users.Where(u => u.Email.ToLower().Contains(term) || u.FullName.ToLower().Contains(term)).ToList();
        }

        var allUserRoles = (await _uow.UserRoles.GetAllAsync(ct)).ToList();
        var allRoles = (await _uow.Roles.GetAllAsync(ct)).ToList();

        if (roleId.HasValue)
        {
            var userIdsWithRole = allUserRoles.Where(ur => ur.RoleId == roleId.Value).Select(ur => ur.UserId).ToHashSet();
            users = users.Where(u => userIdsWithRole.Contains(u.Id)).ToList();
        }

        var ordered = users.OrderByDescending(u => u.CreatedAt);
        var mapped = ordered.Select(u => MapUser(u, RolesFor(u.Id, allUserRoles, allRoles)));

        return PagedResult<AdminUserResponse>.Create(mapped, page, pageSize);
    }

    public async Task<AdminUserResponse> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");
        var roles = await GetUserRolesInternalAsync(userId, ct);
        return MapUser(user, roles);
    }

    public async Task<AdminUserResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var role = await _uow.Roles.GetByIdAsync(request.RoleId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy role.");

        var existingUserRoles = await _uow.UserRoles.FindAsync(ur => ur.UserId == userId, ct);
        foreach (var ur in existingUserRoles)
            _uow.UserRoles.Remove(ur);

        await _uow.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = role.Id }, ct);
        await _uow.SaveChangesAsync(ct);

        return MapUser(user, [new RoleResponse { Id = role.Id, Name = role.Name, Description = role.Description }]);
    }

    public async Task<AdminUserResponse> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        var roles = await GetUserRolesInternalAsync(userId, ct);
        return MapUser(user, roles);
    }

    public async Task<AdminUserResponse> CreateDoctorAsync(CreateDoctorRequest request, CancellationToken ct = default)
    {
        if (request.RoleId != 2 && request.RoleId != 3)
            throw new InvalidOperationException("Chỉ có thể tạo tài khoản Bác sĩ hoặc Bệnh nhân.");

        if (await _uow.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _password.Hash(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            IsEmailVerified = false,
        };
        if (request.RoleId == 2)
        {
            user.HospitalId = request.HospitalId;
            user.Specialty = request.Specialty?.Trim();
            user.ConsultationFee = request.ConsultationFee;
        }
        await _uow.Users.AddAsync(user, ct);

        await _uow.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = request.RoleId }, ct);
        await _uow.UserNotificationSettings.AddAsync(new UserNotificationSetting { UserId = user.Id }, ct);

        await _uow.SaveChangesAsync(ct);

        var role = await _uow.Roles.GetByIdAsync(request.RoleId, ct);
        return MapUser(user, [new RoleResponse { Id = request.RoleId, Name = role?.Name ?? "User", Description = role?.Description }]);
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.PasswordHash = _password.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _uow.Roles.GetAllAsync(ct);
        return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name, Description = r.Description });
    }

    public async Task<IEnumerable<PermissionResponse>> GetPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _uow.Permissions.GetAllAsync(ct);
        return permissions.Select(MapPermission);
    }

    public async Task<IEnumerable<int>> GetRolePermissionIdsAsync(int roleId, CancellationToken ct = default)
    {
        var rolePermissions = await _uow.RolePermissions.FindAsync(rp => rp.RoleId == roleId, ct);
        return rolePermissions.Select(rp => rp.PermissionId);
    }

    public async Task UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Roles.GetByIdAsync(roleId, ct) ?? throw new KeyNotFoundException("Không tìm thấy role.");

        var current = (await _uow.RolePermissions.FindAsync(rp => rp.RoleId == roleId, ct)).ToList();
        var requestedIds = request.PermissionIds.ToHashSet();

        foreach (var rp in current.Where(rp => !requestedIds.Contains(rp.PermissionId)))
            _uow.RolePermissions.Remove(rp);

        var currentIds = current.Select(rp => rp.PermissionId).ToHashSet();
        foreach (var permissionId in requestedIds.Where(id => !currentIds.Contains(id)))
            await _uow.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<DoctorPatientResponse>> GetDoctorPatientsAsync(Guid? doctorId, CancellationToken ct = default)
    {
        var assignments = doctorId.HasValue
            ? await _uow.DoctorPatients.FindAsync(dp => dp.DoctorId == doctorId.Value && dp.IsActive, ct)
            : await _uow.DoctorPatients.FindAsync(dp => dp.IsActive, ct);

        var list = assignments.OrderByDescending(a => a.CreatedAt).ToList();
        var userIds = list.SelectMany(a => new[] { a.DoctorId, a.PatientId }).Distinct().ToList();
        var users = (await _uow.Users.FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        return list.Select(a => new DoctorPatientResponse
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = users.TryGetValue(a.DoctorId, out var doc) ? doc.FullName : "(đã xoá)",
            PatientId = a.PatientId,
            PatientName = users.TryGetValue(a.PatientId, out var pat) ? pat.FullName : "(đã xoá)",
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
        });
    }

    public async Task<DoctorPatientResponse> AssignPatientAsync(AssignPatientRequest request, Guid actorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Users.GetByIdAsync(request.DoctorId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy bác sĩ.");
        var patient = await _uow.Users.GetByIdAsync(request.PatientId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy bệnh nhân.");

        var doctorRoles = await GetUserRolesInternalAsync(request.DoctorId, ct);
        if (!doctorRoles.Any(r => r.Id == 2))
            throw new InvalidOperationException("Người dùng này không có role Bác sĩ.");

        var existing = await _uow.DoctorPatients.FirstOrDefaultAsync(
            dp => dp.DoctorId == request.DoctorId && dp.PatientId == request.PatientId, ct);

        DoctorPatient assignment;
        if (existing is not null)
        {
            if (existing.IsActive)
                throw new InvalidOperationException("Bệnh nhân đã được gán cho bác sĩ này rồi.");

            existing.IsActive = true;
            existing.AssignedBy = actorId;
            _uow.DoctorPatients.Update(existing);
            assignment = existing;
        }
        else
        {
            assignment = new DoctorPatient
            {
                Id = Guid.NewGuid(),
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                AssignedBy = actorId,
                IsActive = true,
            };
            await _uow.DoctorPatients.AddAsync(assignment, ct);
        }

        await _uow.SaveChangesAsync(ct);

        return new DoctorPatientResponse
        {
            Id = assignment.Id,
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            PatientId = patient.Id,
            PatientName = patient.FullName,
            IsActive = true,
            CreatedAt = assignment.CreatedAt,
        };
    }

    public async Task UnassignPatientAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var assignment = await _uow.DoctorPatients.GetByIdAsync(assignmentId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy phân công.");

        assignment.IsActive = false;
        _uow.DoctorPatients.Update(assignment);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, Guid? userId, string? entity, CancellationToken ct = default)
    {
        var logs = (await _uow.AuditLogs.GetAllAsync(ct)).AsEnumerable();

        if (userId.HasValue)
            logs = logs.Where(l => l.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(entity))
            logs = logs.Where(l => l.Entity == entity);

        var ordered = logs.OrderByDescending(l => l.CreatedAt).ToList();

        var userIds = ordered.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().ToList();
        var users = (await _uow.Users.FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        var mapped = ordered.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserId.HasValue && users.TryGetValue(l.UserId.Value, out var u) ? u.Email : null,
            Action = l.Action,
            Entity = l.Entity,
            EntityId = l.EntityId,
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            IpAddress = l.IpAddress,
            CreatedAt = l.CreatedAt,
        });

        return PagedResult<AuditLogResponse>.Create(mapped, page, pageSize);
    }

    public async Task<IEnumerable<SystemSettingResponse>> GetSystemSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _uow.SystemSettings.GetAllAsync(ct);
        return settings.OrderBy(s => s.SettingKey).Select(MapSetting);
    }

    public async Task<SystemSettingResponse> UpdateSystemSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken ct = default)
    {
        var setting = await _uow.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy cấu hình.");

        setting.SettingValue = request.SettingValue;
        setting.UpdatedAt = DateTime.UtcNow;
        _uow.SystemSettings.Update(setting);
        await _uow.SaveChangesAsync(ct);

        return MapSetting(setting);
    }

    public async Task<AdminDashboardResponse> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var allUserRoles = (await _uow.UserRoles.GetAllAsync(ct)).ToList();
        var totalUsers = await _uow.Users.CountAsync(null, ct);
        var totalDoctors = allUserRoles.Count(ur => ur.RoleId == 2);
        var totalPatients = allUserRoles.Count(ur => ur.RoleId == 3);
        var totalAssignments = await _uow.DoctorPatients.CountAsync(dp => dp.IsActive, ct);
        var recentAuditLogCount = await _uow.AuditLogs.CountAsync(
            l => l.CreatedAt >= DateTime.UtcNow.AddDays(-7), ct);

        return new AdminDashboardResponse
        {
            TotalUsers = totalUsers,
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalAssignments = totalAssignments,
            RecentAuditLogCount = recentAuditLogCount,
        };
    }

    public async Task<PagedResult<AppointmentResponse>> GetAllAppointmentsAsync(int page, int pageSize, AppointmentStatus? status, CancellationToken ct = default)
    {
        var appointments = await _uow.Appointments.FindAsync(a => status == null || a.Status == status, ct);
        var ordered = appointments.OrderByDescending(a => a.AppointmentTime).ToList();

        var userIds = ordered.SelectMany(a => new[] { a.DoctorId, a.PatientId }).Distinct().ToList();
        var users = (await _uow.Users.FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        var mapped = ordered.Select(a => new AppointmentResponse
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = users.TryGetValue(a.DoctorId, out var doc) ? doc.FullName : "(đã xoá)",
            PatientId = a.PatientId,
            PatientName = users.TryGetValue(a.PatientId, out var pat) ? pat.FullName : "(đã xoá)",
            AppointmentTime = a.AppointmentTime,
            Reason = a.Reason,
            Status = a.Status,
            Notes = a.Notes,
            Fee = a.Fee,
            IsPaid = a.IsPaid,
            CreatedAt = a.CreatedAt,
        });

        return PagedResult<AppointmentResponse>.Create(mapped, page, pageSize);
    }

    private async Task<List<RoleResponse>> GetUserRolesInternalAsync(Guid userId, CancellationToken ct)
    {
        var userRoles = await _uow.UserRoles.FindAsync(ur => ur.UserId == userId, ct);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _uow.Roles.FindAsync(r => roleIds.Contains(r.Id), ct);
        return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name, Description = r.Description }).ToList();
    }

    private static List<RoleResponse> RolesFor(Guid userId, List<UserRole> allUserRoles, List<Role> allRoles)
    {
        var roleIds = allUserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToHashSet();
        return allRoles.Where(r => roleIds.Contains(r.Id))
            .Select(r => new RoleResponse { Id = r.Id, Name = r.Name, Description = r.Description })
            .ToList();
    }

    private static AdminUserResponse MapUser(User u, IEnumerable<RoleResponse> roles) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        PhoneNumber = u.PhoneNumber,
        IsActive = u.IsActive,
        IsEmailVerified = u.IsEmailVerified,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt,
        Roles = roles,
    };

    private static PermissionResponse MapPermission(Permission p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Resource = p.Resource,
        Action = p.Action,
        Description = p.Description,
    };

    private static SystemSettingResponse MapSetting(SystemSetting s) => new()
    {
        Id = s.Id,
        SettingKey = s.SettingKey,
        SettingValue = s.SettingValue,
        Description = s.Description,
        UpdatedAt = s.UpdatedAt,
    };
}
