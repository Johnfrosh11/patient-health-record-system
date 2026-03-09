using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Application.Services.Audit;

/// <summary>
/// Audit service interface - handles logging and retrieval of audit events
/// </summary>
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

    Task<List<TAuditLog>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        CancellationToken cancellationToken = default);

    Task<List<TAuditLog>> GetEntityAuditLogsAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<List<TAuditLog>> GetUserAuditLogsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
