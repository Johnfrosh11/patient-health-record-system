using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Application.Services.HealthRecords;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Health Records controller - sealed, thin actions only
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public sealed class HealthRecordsController(IHealthRecordService svc) : BaseController
{
    /// <summary>
    /// Create a new health record (requires createPatientRecords permission)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Policy = Permissions.CreatePatientRecords)]
    public async Task<IActionResult> Create([FromBody] CreateHealthRecordDto model, CancellationToken ct = default)
        => Response(await svc.CreateHealthRecordAsync(model, ct));

    /// <summary>
    /// Get health record by ID (viewPatientRecords or own record)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Response(await svc.GetHealthRecordByIdAsync(id, ct));

    /// <summary>
    /// Get all health records with pagination (requires viewPatientRecords permission)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = Permissions.ViewPatientRecords)]
    public async Task<IActionResult> GetAll([FromQuery] BaseSearchDto payload, CancellationToken ct = default)
        => Response(await svc.GetAllHealthRecordsAsync(payload, ct));

    /// <summary>
    /// Update a health record (own records only)
    /// </summary>
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateHealthRecordDto model, CancellationToken ct = default)
        => Response(await svc.UpdateHealthRecordAsync(model, ct));

    /// <summary>
    /// Soft delete a health record (own records only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        => Response(await svc.DeleteHealthRecordAsync(id, ct));
}
