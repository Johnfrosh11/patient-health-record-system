using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;

namespace PatientHealthRecord.Application.Services.AccessRequests;

/// <summary>
/// Access Request service implementation - handles time-bound access requests
/// </summary>
public sealed class AccessRequestService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    ILogger<AccessRequestService> logger
) : IAccessRequestService
{
    public async Task<List<AccessRequestResponse>> GetAllAsync(
        Guid currentUserId, 
        bool isApprover, 
        CancellationToken cancellationToken = default)
    {
        var query = db.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Include(ar => ar.Reviewer)
            .Where(ar => ar.IsActive);

        // If user is an approver, show all; otherwise show only their own requests
        if (!isApprover)
        {
            query = query.Where(ar => ar.RequestingUserId == currentUserId);
        }

        var requests = await query
            .OrderByDescending(ar => ar.RequestDate)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToResponse).ToList();
    }

    public async Task<AccessRequestResponse> GetByIdAsync(
        Guid id, 
        Guid currentUserId, 
        CancellationToken cancellationToken = default)
    {
        var request = await db.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Include(ar => ar.Reviewer)
            .FirstOrDefaultAsync(ar => ar.AccessRequestId == id && ar.IsActive, cancellationToken);

        if (request == null)
            throw new KeyNotFoundException($"Access request {id} not found.");

        return MapToResponse(request);
    }

    public async Task<List<AccessRequestResponse>> GetPendingAsync(
        Guid currentUserId, 
        CancellationToken cancellationToken = default)
    {
        var requests = await db.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .Where(ar => ar.Status == AccessRequestStatus.Pending && ar.IsActive)
            .OrderBy(ar => ar.RequestDate)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToResponse).ToList();
    }

    public async Task<AccessRequestResponse> CreateAsync(
        CreateAccessRequestRequest request, 
        Guid currentUserId, 
        CancellationToken cancellationToken = default)
    {
        // Validate health record exists
        var healthRecord = await db.HealthRecords
            .FirstOrDefaultAsync(h => h.HealthRecordId == request.HealthRecordId && h.IsActive, cancellationToken);

        if (healthRecord == null)
            throw new KeyNotFoundException($"Health record {request.HealthRecordId} not found.");

        // Check if user already has a pending request for this record
        var existingRequest = await db.AccessRequests
            .AnyAsync(ar => 
                ar.HealthRecordId == request.HealthRecordId && 
                ar.RequestingUserId == currentUserId && 
                ar.Status == AccessRequestStatus.Pending && 
                ar.IsActive, cancellationToken);

        if (existingRequest)
            throw new InvalidOperationException("You already have a pending request for this record.");

        var accessRequest = new TAccessRequest
        {
            HealthRecordId = request.HealthRecordId,
            RequestingUserId = currentUserId,
            Reason = request.Reason,
            RequestDate = DateTime.UtcNow,
            Status = AccessRequestStatus.Pending,
            OrganizationId = authUser.OrganizationId,
            CreatedBy = currentUserId.ToString(),
            IpAddress = authUser.IpAddress
        };

        await db.AccessRequests.AddAsync(accessRequest, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Access request {RequestId} created by user {UserId} for record {RecordId}",
            accessRequest.AccessRequestId, currentUserId, request.HealthRecordId);

        // Reload with navigation properties
        await db.Entry(accessRequest).Reference(ar => ar.HealthRecord).LoadAsync(cancellationToken);
        await db.Entry(accessRequest).Reference(ar => ar.RequestingUser).LoadAsync(cancellationToken);

        return MapToResponse(accessRequest);
    }

    public async Task<AccessRequestResponse> ApproveAsync(
        Guid id, 
        ApproveAccessRequestRequest request, 
        Guid reviewerId, 
        CancellationToken cancellationToken = default)
    {
        var accessRequest = await db.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .FirstOrDefaultAsync(ar => ar.AccessRequestId == id && ar.IsActive, cancellationToken);

        if (accessRequest == null)
            throw new KeyNotFoundException($"Access request {id} not found.");

        if (accessRequest.Status != AccessRequestStatus.Pending)
            throw new InvalidOperationException($"Access request is already {accessRequest.Status}.");

        // Validate time range
        if (request.AccessStartDateTime >= request.AccessEndDateTime)
            throw new ArgumentException("End date must be after start date.");

        if (request.AccessEndDateTime <= DateTime.UtcNow)
            throw new ArgumentException("End date must be in the future.");

        accessRequest.Status = AccessRequestStatus.Approved;
        accessRequest.ReviewedBy = reviewerId;
        accessRequest.ReviewedDate = DateTime.UtcNow;
        accessRequest.ReviewComment = request.ReviewComment;
        accessRequest.AccessStartDateTime = request.AccessStartDateTime;
        accessRequest.AccessEndDateTime = request.AccessEndDateTime;
        accessRequest.LastModified = DateTime.UtcNow;
        accessRequest.ModifiedBy = reviewerId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        // Load reviewer
        await db.Entry(accessRequest).Reference(ar => ar.Reviewer).LoadAsync(cancellationToken);

        logger.LogInformation(
            "Access request {RequestId} approved by {ReviewerId}. Access valid from {Start} to {End}",
            id, reviewerId, request.AccessStartDateTime, request.AccessEndDateTime);

        return MapToResponse(accessRequest);
    }

    public async Task<AccessRequestResponse> DeclineAsync(
        Guid id, 
        DeclineAccessRequestRequest request, 
        Guid reviewerId, 
        CancellationToken cancellationToken = default)
    {
        var accessRequest = await db.AccessRequests
            .Include(ar => ar.HealthRecord)
            .Include(ar => ar.RequestingUser)
            .FirstOrDefaultAsync(ar => ar.AccessRequestId == id && ar.IsActive, cancellationToken);

        if (accessRequest == null)
            throw new KeyNotFoundException($"Access request {id} not found.");

        if (accessRequest.Status != AccessRequestStatus.Pending)
            throw new InvalidOperationException($"Access request is already {accessRequest.Status}.");

        accessRequest.Status = AccessRequestStatus.Rejected;
        accessRequest.ReviewedBy = reviewerId;
        accessRequest.ReviewedDate = DateTime.UtcNow;
        accessRequest.ReviewComment = request.ReviewComment;
        accessRequest.LastModified = DateTime.UtcNow;
        accessRequest.ModifiedBy = reviewerId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        // Load reviewer
        await db.Entry(accessRequest).Reference(ar => ar.Reviewer).LoadAsync(cancellationToken);

        logger.LogInformation(
            "Access request {RequestId} declined by {ReviewerId}. Reason: {Comment}",
            id, reviewerId, request.ReviewComment);

        return MapToResponse(accessRequest);
    }

    private static AccessRequestResponse MapToResponse(TAccessRequest ar)
    {
        var now = DateTime.UtcNow;
        var isAccessActive = ar.Status == AccessRequestStatus.Approved &&
                             ar.AccessStartDateTime <= now &&
                             ar.AccessEndDateTime >= now;

        return new AccessRequestResponse
        {
            Id = ar.AccessRequestId,
            HealthRecordId = ar.HealthRecordId,
            PatientName = ar.HealthRecord?.PatientName ?? string.Empty,
            RequestingUserId = ar.RequestingUserId,
            RequestingUsername = ar.RequestingUser?.Username ?? string.Empty,
            Reason = ar.Reason,
            RequestDate = ar.RequestDate,
            Status = ar.Status,
            StatusText = ar.Status.ToString(),
            ReviewedBy = ar.ReviewedBy,
            ReviewedByUsername = ar.Reviewer?.Username,
            ReviewedDate = ar.ReviewedDate,
            ReviewComment = ar.ReviewComment,
            AccessStartDateTime = ar.AccessStartDateTime,
            AccessEndDateTime = ar.AccessEndDateTime,
            IsAccessActive = isAccessActive,
            CreatedAt = ar.CreatedDate
        };
    }
}
