using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace PatientHealthRecord.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access-requests")]
public class AccessRequestsController : BaseController
{
    private readonly IAccessRequestService _accessRequestService;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateAccessRequestRequest> _createValidator;
    private readonly IValidator<ApproveAccessRequestRequest> _approveValidator;
    private readonly IValidator<DeclineAccessRequestRequest> _declineValidator;

    public AccessRequestsController(
        IAccessRequestService accessRequestService,
        IAuditService auditService,
        IValidator<CreateAccessRequestRequest> createValidator,
        IValidator<ApproveAccessRequestRequest> approveValidator,
        IValidator<DeclineAccessRequestRequest> declineValidator)
    {
        _accessRequestService = accessRequestService;
        _auditService = auditService;
        _createValidator = createValidator;
        _approveValidator = approveValidator;
        _declineValidator = declineValidator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all access requests")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        
        var hasApprovePermission = User.Claims
            .Where(c => c.Type == "permission")
            .Any(c => c.Value == Permissions.ApproveAccessRequests);
        
        var response = await _accessRequestService.GetAllAsync(userId, hasApprovePermission);
        return Response(response);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get access request by ID")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var response = await _accessRequestService.GetByIdAsync(id, userId);
        return Response(response);
    }

    [HttpGet("pending")]
    [SwaggerOperation(Summary = "Get all pending access requests")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> GetPending()
    {
        var userId = GetCurrentUserId();
        var response = await _accessRequestService.GetPendingAsync(userId);
        return Response(response);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new access request")]
    public async Task<IActionResult> Create([FromBody] CreateAccessRequestRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return NotifyModelStateError();
        }

        var userId = GetCurrentUserId();
        var response = await _accessRequestService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}/approve")]
    [SwaggerOperation(Summary = "Approve an access request")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveAccessRequestRequest request)
    {
        var validationResult = await _approveValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return NotifyModelStateError();
        }

        var userId = GetCurrentUserId();
        var response = await _accessRequestService.ApproveAsync(id, request, userId);
        
        await _auditService.LogAsync(AuditAction.ApproveAccess, "AccessRequest", id.ToString(), 
            $"Access request approved for health record {response.HealthRecordId}");
        
        return Response(response);
    }

    [HttpPut("{id:guid}/decline")]
    [SwaggerOperation(Summary = "Decline an access request")]
    [Authorize(Policy = Permissions.ApproveAccessRequests)]
    public async Task<IActionResult> Decline(Guid id, [FromBody] DeclineAccessRequestRequest request)
    {
        var validationResult = await _declineValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return NotifyModelStateError();
        }

        var userId = GetCurrentUserId();
        var response = await _accessRequestService.DeclineAsync(id, request, userId);
        
        await _auditService.LogAsync(AuditAction.DeclineAccess, "AccessRequest", id.ToString(), 
            $"Access request declined for health record {response.HealthRecordId}. Reason: {request.ReviewComment}");
        
        return Response(response);
    }
}
