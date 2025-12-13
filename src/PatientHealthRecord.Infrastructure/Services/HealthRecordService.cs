using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;

namespace PatientHealthRecord.Infrastructure.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly ApplicationDbContext _context;

    public HealthRecordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedHealthRecordsResponse> GetAllAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);;
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        var hasViewAllPermission = HasPermission(user, Permissions.ViewPatientRecords);

        var query = _context.HealthRecords
            .Include(hr => hr.Creator)
            .Include(hr => hr.AccessRequests)
            .Where(hr => !hr.IsDeleted)
            .AsQueryable();

        if (!hasViewAllPermission)
        {
            query = query.Where(hr =>
                hr.CreatedBy == currentUserId ||
                hr.AccessRequests.Any(ar =>
                    ar.RequestingUserId == currentUserId &&
                    ar.Status == AccessRequestStatus.Approved &&
                    ar.AccessStartDateTime != null &&
                    ar.AccessEndDateTime != null &&
                    DateTime.UtcNow >= ar.AccessStartDateTime &&
                    DateTime.UtcNow <= ar.AccessEndDateTime));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var records = await query
            .OrderByDescending(hr => hr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(hr => new HealthRecordResponse
            {
                Id = hr.Id,
                PatientName = hr.PatientName,
                DateOfBirth = hr.DateOfBirth,
                Diagnosis = hr.Diagnosis,
                TreatmentPlan = hr.TreatmentPlan,
                MedicalHistory = hr.MedicalHistory,
                CreatedBy = hr.CreatedBy,
                CreatedByUsername = hr.Creator.Username,
                CreatedAt = hr.CreatedAt,
                LastModifiedBy = hr.LastModifiedBy,
                LastModifiedDate = hr.LastModifiedDate,
                IsDeleted = hr.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return new PaginatedHealthRecordsResponse
        {
            Items = records,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<HealthRecordResponse> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var record = await _context.HealthRecords
            .Include(hr => hr.Creator)
            .FirstOrDefaultAsync(hr => hr.Id == id && !hr.IsDeleted, cancellationToken);

        if (record == null)
        {
            throw new NotFoundException("Health record", id);
        }

        if (!await CanUserAccessRecordAsync(id, currentUserId, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to view this health record");
        }

        return new HealthRecordResponse
        {
            Id = record.Id,
            PatientName = record.PatientName,
            DateOfBirth = record.DateOfBirth,
            Diagnosis = record.Diagnosis,
            TreatmentPlan = record.TreatmentPlan,
            MedicalHistory = record.MedicalHistory,
            CreatedBy = record.CreatedBy,
            CreatedByUsername = record.Creator.Username,
            CreatedAt = record.CreatedAt,
            LastModifiedBy = record.LastModifiedBy,
            LastModifiedDate = record.LastModifiedDate,
            IsDeleted = record.IsDeleted
        };
    }

    public async Task<List<HealthRecordResponse>> GetMyRecordsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var records = await _context.HealthRecords
            .Include(hr => hr.Creator)
            .Where(hr => hr.CreatedBy == currentUserId && !hr.IsDeleted)
            .OrderByDescending(hr => hr.CreatedAt)
            .Select(hr => new HealthRecordResponse
            {
                Id = hr.Id,
                PatientName = hr.PatientName,
                DateOfBirth = hr.DateOfBirth,
                Diagnosis = hr.Diagnosis,
                TreatmentPlan = hr.TreatmentPlan,
                MedicalHistory = hr.MedicalHistory,
                CreatedBy = hr.CreatedBy,
                CreatedByUsername = hr.Creator.Username,
                CreatedAt = hr.CreatedAt,
                LastModifiedBy = hr.LastModifiedBy,
                LastModifiedDate = hr.LastModifiedDate,
                IsDeleted = hr.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return records;
    }

    public async Task<HealthRecordResponse> CreateAsync(CreateHealthRecordRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);;
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (!HasPermission(user, Permissions.CreatePatientRecords))
        {
            throw new ForbiddenException("You do not have permission to create patient records");
        }

        var healthRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = request.PatientName,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
            Diagnosis = request.Diagnosis,
            TreatmentPlan = request.TreatmentPlan,
            MedicalHistory = request.MedicalHistory,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(healthRecord).Reference(hr => hr.Creator).LoadAsync(cancellationToken);

        return new HealthRecordResponse
        {
            Id = healthRecord.Id,
            PatientName = healthRecord.PatientName,
            DateOfBirth = healthRecord.DateOfBirth,
            Diagnosis = healthRecord.Diagnosis,
            TreatmentPlan = healthRecord.TreatmentPlan,
            MedicalHistory = healthRecord.MedicalHistory,
            CreatedBy = healthRecord.CreatedBy,
            CreatedByUsername = healthRecord.Creator.Username,
            CreatedAt = healthRecord.CreatedAt,
            LastModifiedBy = healthRecord.LastModifiedBy,
            LastModifiedDate = healthRecord.LastModifiedDate,
            IsDeleted = healthRecord.IsDeleted
        };
    }

    public async Task<HealthRecordResponse> UpdateAsync(Guid id, UpdateHealthRecordRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var record = await _context.HealthRecords
            .Include(hr => hr.Creator)
            .FirstOrDefaultAsync(hr => hr.Id == id && !hr.IsDeleted, cancellationToken);

        if (record == null)
        {
            throw new NotFoundException("Health record", id);
        }

        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);;
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (!HasPermission(user, Permissions.UpdatePatientRecords))
        {
            throw new ForbiddenException("You do not have permission to update patient records");
        }

        if (record.CreatedBy != currentUserId)
        {
            throw new ForbiddenException("You can only update records you created");
        }

        record.PatientName = request.PatientName;
        record.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc);
        record.Diagnosis = request.Diagnosis;
        record.TreatmentPlan = request.TreatmentPlan;
        record.MedicalHistory = request.MedicalHistory;
        record.LastModifiedBy = currentUserId;
        record.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new HealthRecordResponse
        {
            Id = record.Id,
            PatientName = record.PatientName,
            DateOfBirth = record.DateOfBirth,
            Diagnosis = record.Diagnosis,
            TreatmentPlan = record.TreatmentPlan,
            MedicalHistory = record.MedicalHistory,
            CreatedBy = record.CreatedBy,
            CreatedByUsername = record.Creator.Username,
            CreatedAt = record.CreatedAt,
            LastModifiedBy = record.LastModifiedBy,
            LastModifiedDate = record.LastModifiedDate,
            IsDeleted = record.IsDeleted
        };
    }

    public async Task<DeleteHealthRecordResponse> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var record = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.Id == id && !hr.IsDeleted, cancellationToken);

        if (record == null)
        {
            throw new NotFoundException("Health record", id);
        }

        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        var hasDeletePermission = HasPermission(user, Permissions.DeletePatientRecords);
        var isOwner = record.CreatedBy == currentUserId;

        if (!hasDeletePermission && !isOwner)
        {
            throw new ForbiddenException("You can only delete records you created or need delete permission");
        }

        var deletedAt = DateTime.UtcNow;
        record.IsDeleted = true;
        record.DeletedBy = currentUserId;
        record.DeletedDate = deletedAt;

        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteHealthRecordResponse
        {
            Id = record.Id,
            PatientName = record.PatientName,
            DeletedAt = deletedAt,
            DeletedBy = currentUserId,
            Message = "Patient record has been soft deleted successfully. The record is archived and can be restored if needed."
        };
    }

    public async Task<bool> CanUserAccessRecordAsync(Guid recordId, Guid userId, CancellationToken cancellationToken = default)
    {
        var record = await _context.HealthRecords
            .Include(hr => hr.AccessRequests)
            .FirstOrDefaultAsync(hr => hr.Id == recordId && !hr.IsDeleted, cancellationToken);

        if (record == null)
        {
            return false;
        }

        if (record.CreatedBy == userId)
        {
            return true;
        }

        var user = await GetUserWithPermissionsAsync(userId, cancellationToken);
        if (user != null && HasPermission(user, Permissions.ViewPatientRecords))
        {
            return true;
        }

        var hasActiveAccess = record.AccessRequests.Any(ar =>
            ar.RequestingUserId == userId &&
            ar.Status == AccessRequestStatus.Approved &&
            ar.AccessStartDateTime != null &&
            ar.AccessEndDateTime != null &&
            DateTime.UtcNow >= ar.AccessStartDateTime &&
            DateTime.UtcNow <= ar.AccessEndDateTime);

        return hasActiveAccess;
    }

    private async Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    private bool HasPermission(User user, string permissionName)
    {
        return user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == permissionName);
    }
}
