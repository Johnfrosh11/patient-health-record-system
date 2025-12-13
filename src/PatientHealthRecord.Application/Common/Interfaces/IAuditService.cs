using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        string? details = null,
        CancellationToken cancellationToken = default);

    Task LogViewAsync(
        string entityName,
        string entityId,
        string? details = null,
        CancellationToken cancellationToken = default);

    Task<List<AuditLog>> GetEntityAuditLogsAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<List<AuditLog>> GetUserAuditLogsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<List<AuditLog>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        CancellationToken cancellationToken = default);
}
