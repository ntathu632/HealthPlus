using System.Security.Claims;
using System.Text.Json;
using HealthPlus.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HealthPlus.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContext;

    // Entities to exclude from audit (high-volume or system tables)
    private static readonly HashSet<Type> _excludedTypes =
    [
        typeof(AuditLog),
        typeof(ReminderLog),
        typeof(RefreshToken),
    ];

    public AuditInterceptor(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditLogs = BuildAuditLogs(context);
        if (auditLogs.Count > 0)
            await context.Set<AuditLog>().AddRangeAsync(auditLogs, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> BuildAuditLogs(DbContext context)
    {
        var userId = GetCurrentUserId();
        var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();
        var logs = new List<AuditLog>();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && !_excludedTypes.Contains(e.Entity.GetType()));

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            var entityId = GetPrimaryKey(entry);
            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var old = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                var @new = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                oldValues = JsonSerializer.Serialize(old);
                newValues = JsonSerializer.Serialize(@new);
            }
            else if (entry.State == EntityState.Added)
            {
                var @new = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(@new);
            }

            logs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entry.Entity.GetType().Name,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ip,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
            });
        }

        return logs;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? _httpContext.HttpContext?.User?.FindFirst("sub");
        return Guid.TryParse(claim?.Value, out var id) ? id : null;
    }

    private static string? GetPrimaryKey(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return null;
        var values = key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString());
        return string.Join(",", values);
    }
}
