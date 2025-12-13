using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Application.DTOs.HealthRecords;

public class CreateHealthRecordRequest
{
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
}

public class UpdateHealthRecordRequest
{
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
}

public class HealthRecordResponse
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class PaginatedHealthRecordsResponse
{
    public List<HealthRecordResponse> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

public class DeleteHealthRecordResponse
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public Guid DeletedBy { get; set; }
    public string Message { get; set; } = string.Empty;
}
