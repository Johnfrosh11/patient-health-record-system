using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Application.Services.AccessRequests;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Access Requests controller - time-bound access to health records
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public sealed class AccessRequestsController(IAccessRequestService svc) : BaseController
{
    /// <summary>
    /// Get all access requests (user sees their own, approvers see pending for review)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool isApprover = false, CancellationToken ct = default)
        => Ok(await svc.GetAllAsync(GetCurrentUserId(), isApprover, ct));

    /// <summary>
    /// Get access request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(await svc.GetByIdAsync(id, GetCurrentUserId(), ct));

    /// <summary>
    /// Get pending access requests for approval (requires approveAccessRequests permission)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> GetPending(CancellationToken ct = default)
        => Ok(await svc.GetPendingAsync(GetCurrentUserId(), ct));

    /// <summary>
    /// Create a new access request for a health record
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccessRequestRequest request, CancellationToken ct = default)
        => Ok(await svc.CreateAsync(request, GetCurrentUserId(), ct));

    /// <summary>
    /// Approve an access request with time-bound access (requires approveAccessRequests permission)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveAccessRequestRequest request, CancellationToken ct = default)
        => Ok(await svc.ApproveAsync(id, request, GetCurrentUserId(), ct));

    /// <summary>
    /// Decline an access request (requires approveAccessRequests permission)
    /// </summary>
    [HttpPost("{id:guid}/decline")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> Decline(Guid id, [FromBody] DeclineAccessRequestRequest request, CancellationToken ct = default)
        => Ok(await svc.DeclineAsync(id, request, GetCurrentUserId(), ct));
}
