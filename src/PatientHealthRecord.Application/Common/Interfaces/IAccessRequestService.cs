using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Application.Common.Interfaces;

public interface IAccessRequestService
{
    Task<List<AccessRequestResponse>> GetAllAsync(Guid currentUserId, bool isApprover, CancellationToken cancellationToken = default);
    Task<AccessRequestResponse> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<List<AccessRequestResponse>> GetPendingAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task<AccessRequestResponse> CreateAsync(CreateAccessRequestRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<AccessRequestResponse> ApproveAsync(Guid id, ApproveAccessRequestRequest request, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<AccessRequestResponse> DeclineAsync(Guid id, DeclineAccessRequestRequest request, Guid reviewerId, CancellationToken cancellationToken = default);
}
