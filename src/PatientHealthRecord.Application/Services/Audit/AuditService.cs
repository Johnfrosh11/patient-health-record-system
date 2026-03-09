using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;

namespace PatientHealthRecord.Application.Services.Audit;

/// <summary>
/// Audit service implementation - handles logging and retrieval of audit events
/// </summary>
public sealed class AuditService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    ILogger<AuditService> logger
) : IAuditService
{
    public async Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new TAuditLog
        {
            UserId = authUser.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId != null && Guid.TryParse(entityId, out var id) ? id : null,
            Description = details,
            Timestamp = DateTime.UtcNow,
            OrganizationId = authUser.OrganizationId,
            CreatedBy = authUser.UserId.ToString(),
            IpAddress = authUser.IpAddress
        };

        await db.AuditLogs.AddAsync(auditLog, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogDebug(
            "Audit: {Action} on {EntityName}:{EntityId} by user {UserId}",
            action, entityName, entityId, authUser.UserId);
    }

    public async Task LogViewAsync(
        string entityName,
        string entityId,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(AuditAction.Read, entityName, entityId, details, cancellationToken);
    }

    public async Task<List<TAuditLog>> GetEntityAuditLogsAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        Guid? parsedEntityId = Guid.TryParse(entityId, out var id) ? id : null;

        var logs = await db.AuditLogs
            .Where(al => al.EntityName == entityName && 
                         al.EntityId == parsedEntityId && 
                         al.IsActive)
            .OrderByDescending(al => al.Timestamp)
            .Take(100)
            .ToListAsync(cancellationToken);

        return logs;
    }

    public async Task<List<TAuditLog>> GetUserAuditLogsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var logs = await db.AuditLogs
            .Where(al => al.UserId == userId && al.IsActive)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return logs;
    }

    public async Task<List<TAuditLog>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditLogs.Where(al => al.IsActive);

        if (action.HasValue)
        {
            query = query.Where(al => al.Action == action.Value);
        }

        var logs = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return logs;
    }
}
