using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;

namespace PatientHealthRecord.Infrastructure.Services;

public class AccessRequestService : IAccessRequestService
{
    private readonly ApplicationDbContext _context;

    public AccessRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccessRequestResponse>> GetAllAsync(Guid currentUserId, bool isApprover, CancellationToken cancellationToken = default)
    {
        var query = _context.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Include(ar => ar.Reviewer)
            .AsQueryable();

        if (!isApprover)
        {
            query = query.Where(ar => ar.RequestingUserId == currentUserId);
        }

        var requests = await query
            .OrderByDescending(ar => ar.CreatedAt)
            .Select(ar => new AccessRequestResponse
            {
                Id = ar.Id,
                HealthRecordId = ar.HealthRecordId,
                PatientName = ar.HealthRecord.PatientName,
                RequestingUserId = ar.RequestingUserId,
                RequestingUsername = ar.RequestingUser.Username,
                Reason = ar.Reason,
                RequestDate = ar.RequestDate,
                Status = ar.Status,
                StatusText = ar.Status.ToString(),
                ReviewedBy = ar.ReviewedBy,
                ReviewedByUsername = ar.Reviewer != null ? ar.Reviewer.Username : null,
                ReviewedDate = ar.ReviewedDate,
                ReviewComment = ar.ReviewComment,
                AccessStartDateTime = ar.AccessStartDateTime,
                AccessEndDateTime = ar.AccessEndDateTime,
                CreatedAt = ar.CreatedAt,
                IsAccessActive = ar.Status == AccessRequestStatus.Approved &&
                                ar.AccessStartDateTime != null &&
                                ar.AccessEndDateTime != null &&
                                DateTime.UtcNow >= ar.AccessStartDateTime &&
                                DateTime.UtcNow <= ar.AccessEndDateTime
            })
            .ToListAsync(cancellationToken);

        return requests;
    }

    public async Task<AccessRequestResponse> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        var hasApprovePermission = HasPermission(user, Permissions.ApproveAccessRequests);

        var request = await _context.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Include(ar => ar.Reviewer)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

        if (request == null)
        {
            throw new NotFoundException("Access request", id);
        }

        if (!hasApprovePermission && request.RequestingUserId != currentUserId)
        {
            throw new ForbiddenException("You do not have permission to view this access request");
        }

        return new AccessRequestResponse
        {
            Id = request.Id,
            HealthRecordId = request.HealthRecordId,
            PatientName = request.HealthRecord.PatientName,
            RequestingUserId = request.RequestingUserId,
            RequestingUsername = request.RequestingUser.Username,
            Reason = request.Reason,
            RequestDate = request.RequestDate,
            Status = request.Status,
            StatusText = request.Status.ToString(),
            ReviewedBy = request.ReviewedBy,
            ReviewedByUsername = request.Reviewer?.Username,
            ReviewedDate = request.ReviewedDate,
            ReviewComment = request.ReviewComment,
            AccessStartDateTime = request.AccessStartDateTime,
            AccessEndDateTime = request.AccessEndDateTime,
            CreatedAt = request.CreatedAt,
            IsAccessActive = request.Status == AccessRequestStatus.Approved &&
                            request.AccessStartDateTime != null &&
                            request.AccessEndDateTime != null &&
                            DateTime.UtcNow >= request.AccessStartDateTime &&
                            DateTime.UtcNow <= request.AccessEndDateTime
        };
    }

    public async Task<List<AccessRequestResponse>> GetPendingAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (!HasPermission(user, Permissions.ApproveAccessRequests))
        {
            throw new ForbiddenException("You do not have permission to view pending access requests");
        }

        var requests = await _context.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Where(ar => ar.Status == AccessRequestStatus.Pending)
            .OrderBy(ar => ar.CreatedAt)
            .Select(ar => new AccessRequestResponse
            {
                Id = ar.Id,
                HealthRecordId = ar.HealthRecordId,
                PatientName = ar.HealthRecord.PatientName,
                RequestingUserId = ar.RequestingUserId,
                RequestingUsername = ar.RequestingUser.Username,
                Reason = ar.Reason,
                RequestDate = ar.RequestDate,
                Status = ar.Status,
                StatusText = ar.Status.ToString(),
                ReviewedBy = ar.ReviewedBy,
                ReviewedByUsername = null,
                ReviewedDate = ar.ReviewedDate,
                ReviewComment = ar.ReviewComment,
                AccessStartDateTime = ar.AccessStartDateTime,
                AccessEndDateTime = ar.AccessEndDateTime,
                CreatedAt = ar.CreatedAt,
                IsAccessActive = false
            })
            .ToListAsync(cancellationToken);

        return requests;
    }

    public async Task<AccessRequestResponse> CreateAsync(CreateAccessRequestRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (HasPermission(user, Permissions.ViewPatientRecords))
        {
            throw new ValidationException("Users with view all patient records permission cannot request individual access");
        }

        var healthRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.Id == request.HealthRecordId && !hr.IsDeleted, cancellationToken);

        if (healthRecord == null)
        {
            throw new NotFoundException("Health record", request.HealthRecordId);
        }

        if (healthRecord.CreatedBy == currentUserId)
        {
            throw new ValidationException("You cannot request access to your own health records");
        }

        var existingPendingRequest = await _context.AccessRequests
            .AnyAsync(ar => ar.HealthRecordId == request.HealthRecordId &&
                           ar.RequestingUserId == currentUserId &&
                           ar.Status == AccessRequestStatus.Pending,
                     cancellationToken);

        if (existingPendingRequest)
        {
            throw new ConflictException("You already have a pending access request for this health record");
        }

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = request.HealthRecordId,
            RequestingUserId = currentUserId,
            Reason = request.Reason,
            RequestDate = DateTime.UtcNow,
            Status = AccessRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(accessRequest).Reference(ar => ar.HealthRecord).LoadAsync(cancellationToken);
        await _context.Entry(accessRequest).Reference(ar => ar.RequestingUser).LoadAsync(cancellationToken);

        return new AccessRequestResponse
        {
            Id = accessRequest.Id,
            HealthRecordId = accessRequest.HealthRecordId,
            PatientName = accessRequest.HealthRecord.PatientName,
            RequestingUserId = accessRequest.RequestingUserId,
            RequestingUsername = accessRequest.RequestingUser.Username,
            Reason = accessRequest.Reason,
            RequestDate = accessRequest.RequestDate,
            Status = accessRequest.Status,
            StatusText = accessRequest.Status.ToString(),
            ReviewedBy = null,
            ReviewedByUsername = null,
            ReviewedDate = null,
            ReviewComment = null,
            AccessStartDateTime = null,
            AccessEndDateTime = null,
            CreatedAt = accessRequest.CreatedAt,
            IsAccessActive = false
        };
    }

    public async Task<AccessRequestResponse> ApproveAsync(Guid id, ApproveAccessRequestRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (!HasPermission(user, Permissions.ApproveAccessRequests))
        {
            throw new ForbiddenException("You do not have permission to approve access requests");
        }

        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

        if (accessRequest == null)
        {
            throw new NotFoundException("Access request", id);
        }

        if (accessRequest.Status != AccessRequestStatus.Pending)
        {
            throw new ValidationException($"Cannot approve access request with status: {accessRequest.Status}");
        }

        if (request.AccessStartDateTime >= request.AccessEndDateTime)
        {
            throw new ValidationException("Access start date/time must be before end date/time");
        }

        if (request.AccessEndDateTime <= DateTime.UtcNow)
        {
            throw new ValidationException("Access end date/time must be in the future");
        }

        accessRequest.Status = AccessRequestStatus.Approved;
        accessRequest.ReviewedBy = currentUserId;
        accessRequest.ReviewedDate = DateTime.UtcNow;
        accessRequest.ReviewComment = request.ReviewComment;
        accessRequest.AccessStartDateTime = DateTime.SpecifyKind(request.AccessStartDateTime, DateTimeKind.Utc);
        accessRequest.AccessEndDateTime = DateTime.SpecifyKind(request.AccessEndDateTime, DateTimeKind.Utc);

        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(accessRequest).Reference(ar => ar.Reviewer).LoadAsync(cancellationToken);

        return new AccessRequestResponse
        {
            Id = accessRequest.Id,
            HealthRecordId = accessRequest.HealthRecordId,
            PatientName = accessRequest.HealthRecord.PatientName,
            RequestingUserId = accessRequest.RequestingUserId,
            RequestingUsername = accessRequest.RequestingUser.Username,
            Reason = accessRequest.Reason,
            RequestDate = accessRequest.RequestDate,
            Status = accessRequest.Status,
            StatusText = accessRequest.Status.ToString(),
            ReviewedBy = accessRequest.ReviewedBy,
            ReviewedByUsername = accessRequest.Reviewer?.Username,
            ReviewedDate = accessRequest.ReviewedDate,
            ReviewComment = accessRequest.ReviewComment,
            AccessStartDateTime = accessRequest.AccessStartDateTime,
            AccessEndDateTime = accessRequest.AccessEndDateTime,
            CreatedAt = accessRequest.CreatedAt,
            IsAccessActive = DateTime.UtcNow >= accessRequest.AccessStartDateTime &&
                            DateTime.UtcNow <= accessRequest.AccessEndDateTime
        };
    }

    public async Task<AccessRequestResponse> DeclineAsync(Guid id, DeclineAccessRequestRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithPermissionsAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", currentUserId);
        }

        if (!HasPermission(user, Permissions.ApproveAccessRequests))
        {
            throw new ForbiddenException("You do not have permission to decline access requests");
        }

        var accessRequest = await _context.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);

        if (accessRequest == null)
        {
            throw new NotFoundException("Access request", id);
        }

        if (accessRequest.Status != AccessRequestStatus.Pending)
        {
            throw new ValidationException($"Cannot decline access request with status: {accessRequest.Status}");
        }

        accessRequest.Status = AccessRequestStatus.Declined;
        accessRequest.ReviewedBy = currentUserId;
        accessRequest.ReviewedDate = DateTime.UtcNow;
        accessRequest.ReviewComment = request.ReviewComment;

        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(accessRequest).Reference(ar => ar.Reviewer).LoadAsync(cancellationToken);

        return new AccessRequestResponse
        {
            Id = accessRequest.Id,
            HealthRecordId = accessRequest.HealthRecordId,
            PatientName = accessRequest.HealthRecord.PatientName,
            RequestingUserId = accessRequest.RequestingUserId,
            RequestingUsername = accessRequest.RequestingUser.Username,
            Reason = accessRequest.Reason,
            RequestDate = accessRequest.RequestDate,
            Status = accessRequest.Status,
            StatusText = accessRequest.Status.ToString(),
            ReviewedBy = accessRequest.ReviewedBy,
            ReviewedByUsername = accessRequest.Reviewer?.Username,
            ReviewedDate = accessRequest.ReviewedDate,
            ReviewComment = accessRequest.ReviewComment,
            AccessStartDateTime = null,
            AccessEndDateTime = null,
            CreatedAt = accessRequest.CreatedAt,
            IsAccessActive = false
        };
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
