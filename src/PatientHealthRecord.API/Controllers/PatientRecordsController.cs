using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace PatientHealthRecord.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patient-records")]
public class PatientRecordsController : BaseController
{
    private readonly IHealthRecordService _healthRecordService;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateHealthRecordRequest> _createValidator;
    private readonly IValidator<UpdateHealthRecordRequest> _updateValidator;

    public PatientRecordsController(
        IHealthRecordService healthRecordService,
        IAuditService auditService,
        IValidator<CreateHealthRecordRequest> createValidator,
        IValidator<UpdateHealthRecordRequest> updateValidator)
    {
        _healthRecordService = healthRecordService;
        _auditService = auditService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all patient health records with pagination")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var response = await _healthRecordService.GetAllAsync(userId, page, pageSize);
        return Response(response);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get specific patient health record by ID")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var response = await _healthRecordService.GetByIdAsync(id, userId);
        
        await _auditService.LogViewAsync("HealthRecord", id.ToString(), $"User viewed patient record for {response.PatientName}");
        
        return Response(response);
    }

    [HttpGet("my-records")]
    [SwaggerOperation(Summary = "Get all patient health records created by current user")]
    public async Task<IActionResult> GetMyRecords()
    {
        var userId = GetCurrentUserId();
        var response = await _healthRecordService.GetMyRecordsAsync(userId);
        return Response(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CreatePatientRecords)]
    [SwaggerOperation(Summary = "Create a new patient health record")]
    public async Task<IActionResult> Create([FromBody] CreateHealthRecordRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        var userId = GetCurrentUserId();
        var response = await _healthRecordService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UpdatePatientRecords)]
    [SwaggerOperation(Summary = "Update existing patient health record")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHealthRecordRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        var userId = GetCurrentUserId();
        var response = await _healthRecordService.UpdateAsync(id, request, userId);
        return Response(response);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete patient health record (soft delete)")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var response = await _healthRecordService.DeleteAsync(id, userId);
        return Response(response, response.Message);
    }
}
