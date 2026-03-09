using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Application.Services.HealthRecords;

/// <summary>
/// Health Record service interface
/// </summary>
public interface IHealthRecordService
{
    Task<ResponseModel<HealthRecordViewModel>> CreateHealthRecordAsync(CreateHealthRecordDto model, CancellationToken ct = default);
    Task<ResponseModel<HealthRecordViewModel>> GetHealthRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<ResponseModel<List<HealthRecordViewModel>>> GetAllHealthRecordsAsync(BaseSearchDto payload, CancellationToken ct = default);
    Task<ResponseModel<HealthRecordViewModel>> UpdateHealthRecordAsync(UpdateHealthRecordDto model, CancellationToken ct = default);
    Task<ResponseModel<bool>> DeleteHealthRecordAsync(Guid id, CancellationToken ct = default);
}
