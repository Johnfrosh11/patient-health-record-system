using PatientHealthRecord.Application.DTOs.HealthRecords;

namespace PatientHealthRecord.Application.Common.Interfaces;

public interface IHealthRecordService
{
    Task<PaginatedHealthRecordsResponse> GetAllAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<HealthRecordResponse> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<List<HealthRecordResponse>> GetMyRecordsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task<HealthRecordResponse> CreateAsync(CreateHealthRecordRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<HealthRecordResponse> UpdateAsync(Guid id, UpdateHealthRecordRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<DeleteHealthRecordResponse> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessRecordAsync(Guid recordId, Guid userId, CancellationToken cancellationToken = default);
}
