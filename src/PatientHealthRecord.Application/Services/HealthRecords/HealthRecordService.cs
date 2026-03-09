using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Application.Services.HealthRecords;

/// <summary>
/// Health Record service - sealed, uses primary constructor
/// </summary>
public sealed class HealthRecordService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    ILogger<HealthRecordService> logger
) : IHealthRecordService
{
    public async Task<ResponseModel<HealthRecordViewModel>> CreateHealthRecordAsync(
        CreateHealthRecordDto model, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Null check
            if (model == null) return Fail<HealthRecordViewModel>("Request body is required.");

            // Step 2: Business rule - check age is valid
            if (model.DateOfBirth > DateTime.UtcNow)
                return Fail<HealthRecordViewModel>("Date of birth cannot be in the future.");

            // Step 3: Create entity — OrganizationId ALWAYS from authUser
            var entity = new THealthRecord
            {
                PatientName = model.PatientName.Trim(),
                DateOfBirth = model.DateOfBirth,
                Diagnosis = model.Diagnosis.Trim(),
                TreatmentPlan = model.TreatmentPlan.Trim(),
                MedicalHistory = model.MedicalHistory?.Trim(),
                CreatedByUserId = authUser.UserId,
                OrganizationId = authUser.OrganizationId,
                CreatedBy = authUser.UserId.ToString(),
                CreatedDate = DateTime.UtcNow,
            };

            // Step 4: Persist
            await db.HealthRecords.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Health record {RecordId} created for patient {PatientName} by {UserId}. CorrelationId: {CorrelationId}",
                entity.HealthRecordId, entity.PatientName, authUser.UserId, authUser.CorrelationId);

            // Step 5 & 6: Map and return
            return Success("Health record created successfully.", MapToViewModel(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error creating health record for org {OrganizationId}",
                authUser.OrganizationId);
            return Fail<HealthRecordViewModel>("An error occurred. Please try again.");
        }
    }

    public async Task<ResponseModel<HealthRecordViewModel>> GetHealthRecordByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        try
        {
            var entity = await db.HealthRecords
                .Include(h => h.Creator)
                .Where(e => e.OrganizationId == authUser.OrganizationId && e.IsActive)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.HealthRecordId == id, ct);

            if (entity == null)
                return Fail<HealthRecordViewModel>("Health record not found.");

            // Check if user has access to this record
            var hasAccess = await CheckUserAccessAsync(id, ct);

            var vm = MapToViewModel(entity);
            vm.CanAccess = hasAccess;
            vm.CreatedBy = $"{entity.Creator.FirstName} {entity.Creator.LastName}";

            return Success(string.Empty, vm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching health record {Id}", id);
            return Fail<HealthRecordViewModel>("An error occurred.");
        }
    }

    public async Task<ResponseModel<List<HealthRecordViewModel>>> GetAllHealthRecordsAsync(
        BaseSearchDto payload, CancellationToken ct = default)
    {
        try
        {
            var query = db.HealthRecords
                .Include(h => h.Creator)
                .Where(h => h.OrganizationId == authUser.OrganizationId && h.IsActive)
                .AsNoTracking();

            // Search filter
            if (!string.IsNullOrWhiteSpace(payload.Search))
            {
                var search = payload.Search.ToLower();
                query = query.Where(h =>
                    h.PatientName.ToLower().Contains(search) ||
                    h.Diagnosis.ToLower().Contains(search));
            }

            var records = await query
                .OrderByDescending(h => h.CreatedDate)
                .Paginate(payload.PageNumber, payload.PageSize)
                .Select(h => new HealthRecordViewModel
                {
                    HealthRecordId = h.HealthRecordId,
                    PatientName = h.PatientName,
                    DateOfBirth = h.DateOfBirth,
                    Age = DateTime.UtcNow.Year - h.DateOfBirth.Year,
                    Diagnosis = h.Diagnosis,
                    TreatmentPlan = h.TreatmentPlan,
                    MedicalHistory = h.MedicalHistory,
                    CreatedBy = $"{h.Creator.FirstName} {h.Creator.LastName}",
                    CreatedDate = h.CreatedDate.ToString("dd MMM yyyy HH:mm"),
                    LastModified = h.LastModified.HasValue ? h.LastModified.Value.ToString("dd MMM yyyy HH:mm") : null,
                    CanAccess = h.CreatedByUserId == authUser.UserId // Simplified access check
                })
                .ToListAsync(ct);

            return Success(string.Empty, records);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching health records");
            return Fail<List<HealthRecordViewModel>>("An error occurred.");
        }
    }

    public async Task<ResponseModel<HealthRecordViewModel>> UpdateHealthRecordAsync(
        UpdateHealthRecordDto model, CancellationToken ct = default)
    {
        try
        {
            if (model == null) return Fail<HealthRecordViewModel>("Request body is required.");

            var entity = await db.HealthRecords
                .Where(h => h.OrganizationId == authUser.OrganizationId &&h.IsActive)
                .FirstOrDefaultAsync(h => h.HealthRecordId == model.HealthRecordId, ct);

            if (entity == null)
                return Fail<HealthRecordViewModel>("Health record not found.");

            // Check if user has permission to update
            if (entity.CreatedByUserId != authUser.UserId)
                return Fail<HealthRecordViewModel>("You do not have permission to update this record.");

            // Update fields
            entity.PatientName = model.PatientName.Trim();
            entity.DateOfBirth = model.DateOfBirth;
            entity.Diagnosis = model.Diagnosis.Trim();
            entity.TreatmentPlan = model.TreatmentPlan.Trim();
            entity.MedicalHistory = model.MedicalHistory?.Trim();
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = authUser.UserId.ToString();

            db.HealthRecords.Update(entity);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Health record {RecordId} updated by {UserId}", entity.HealthRecordId, authUser.UserId);

            return Success("Health record updated successfully.", MapToViewModel(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating health record");
            return Fail<HealthRecordViewModel>("An error occurred.");
        }
    }

    public async Task<ResponseModel<bool>> DeleteHealthRecordAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var entity = await db.HealthRecords
                .Where(h => h.OrganizationId == authUser.OrganizationId && h.IsActive)
                .FirstOrDefaultAsync(h => h.HealthRecordId == id, ct);

            if (entity == null)
                return Fail<bool>("Health record not found.");

            // Check permission
            if (entity.CreatedByUserId != authUser.UserId)
                return Fail<bool>("You do not have permission to delete this record.");

            // Soft delete
            entity.IsActive = false;
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = authUser.UserId.ToString();

            db.HealthRecords.Update(entity);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Health record {RecordId} deleted by {UserId}", id, authUser.UserId);

            return Success("Health record deleted successfully.", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting health record {Id}", id);
            return Fail<bool>("An error occurred.");
        }
    }

    // ── PRIVATE HELPERS ──────────────────────────────────────────────
    private static HealthRecordViewModel MapToViewModel(THealthRecord e) => new()
    {
        HealthRecordId = e.HealthRecordId,
        PatientName = e.PatientName,
        DateOfBirth = e.DateOfBirth,
        Age = DateTime.UtcNow.Year - e.DateOfBirth.Year,
        Diagnosis = e.Diagnosis,
        TreatmentPlan = e.TreatmentPlan,
        MedicalHistory = e.MedicalHistory,
        CreatedDate = e.CreatedDate.ToString("dd MMM yyyy HH:mm"),
        LastModified = e.LastModified?.ToString("dd MMM yyyy HH:mm"),
    };

    private async Task<bool> CheckUserAccessAsync(Guid recordId, CancellationToken ct)
    {
        // Check if user created the record or has an approved access request
        var isCreator = await db.HealthRecords
            .AnyAsync(h => h.HealthRecordId == recordId && h.CreatedByUserId == authUser.UserId, ct);

        if (isCreator) return true;

        var hasApprovedAccess = await db.AccessRequests
            .AnyAsync(ar =>
                ar.HealthRecordId == recordId &&
                ar.RequestingUserId == authUser.UserId &&
                ar.Status == AccessRequestStatus.Approved &&
                ar.AccessStartDateTime <= DateTime.UtcNow &&
                ar.AccessEndDateTime >= DateTime.UtcNow, ct);

        return hasApprovedAccess;
    }

    private ResponseModel<T> Fail<T>(string message)
        => new() { code = "99", success = false, message = message };

    private ResponseModel<T> Success<T>(string message, T data)
        => new() { code = "00", success = true, message = message, data = data };
}
