using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Audit controller - audit log viewing (admin only - requires manageRoles permission)
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Policy = Permissions.ManageRoles)]
public sealed class AuditController(IAuditService svc) : BaseController
{
    /// <summary>
    /// Get all audit logs with pagination and optional action filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditAction? action = null,
        CancellationToken ct = default)
        => Ok(await svc.GetAllAuditLogsAsync(page, pageSize, action, ct));

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetEntityLogs(string entityName, string entityId, CancellationToken ct = default)
        => Ok(await svc.GetEntityAuditLogsAsync(entityName, entityId, ct));

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserLogs(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await svc.GetUserAuditLogsAsync(userId, page, pageSize, ct));
}
