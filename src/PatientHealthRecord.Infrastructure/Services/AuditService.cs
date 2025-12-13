using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Infrastructure.Data;
using System.Security.Claims;

namespace PatientHealthRecord.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = httpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

        var auditLog = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId != null ? Guid.Parse(userId) : null,
            Username = username,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogViewAsync(
        string entityName,
        string entityId,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(AuditAction.View, entityName, entityId, details, cancellationToken);
    }

    public async Task<List<AuditLog>> GetEntityAuditLogsAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetUserAuditLogsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
