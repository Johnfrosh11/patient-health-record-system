using PatientHealthRecord.Application.DTOs.AccessRequests;

namespace PatientHealthRecord.Application.Services.AccessRequests;

/// <summary>
/// Access Request service interface - handles time-bound access requests
/// </summary>
public interface IAccessRequestService
{
    Task<List<AccessRequestResponse>> GetAllAsync(
        Guid currentUserId, 
        bool isApprover, 
        CancellationToken cancellationToken = default);

    Task<AccessRequestResponse> GetByIdAsync(
        Guid id, 
        Guid currentUserId, 
        CancellationToken cancellationToken = default);

    Task<AccessRequestResponse> CreateAsync(
        CreateAccessRequestRequest request, 
        Guid requestingUserId, 
        CancellationToken cancellationToken = default);

    Task<List<AccessRequestResponse>> GetPendingAsync(
        Guid currentUserId, 
        CancellationToken cancellationToken = default);

    Task<AccessRequestResponse> ApproveAsync(
        Guid accessRequestId, 
        ApproveAccessRequestRequest request, 
        Guid reviewerId, 
        CancellationToken cancellationToken = default);

    Task<AccessRequestResponse> DeclineAsync(
        Guid accessRequestId, 
        DeclineAccessRequestRequest request, 
        Guid reviewerId, 
        CancellationToken cancellationToken = default);
}
