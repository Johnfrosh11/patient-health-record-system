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

// Alias for service layer
public class CreateHealthRecordDto : CreateHealthRecordRequest { }

public class UpdateHealthRecordRequest
{
    public Guid HealthRecordId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
}

// Alias for service layer
public class UpdateHealthRecordDto : UpdateHealthRecordRequest { }

public class HealthRecordResponse
{
    public Guid HealthRecordId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string? LastModified { get; set; }
    public bool CanAccess { get; set; }
}

// Alias for service layer
public class HealthRecordViewModel : HealthRecordResponse { }

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
