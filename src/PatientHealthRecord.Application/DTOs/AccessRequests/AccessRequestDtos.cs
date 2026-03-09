using PatientHealthRecord.Domain;

namespace PatientHealthRecord.Application.DTOs.AccessRequests;

public class CreateAccessRequestRequest
{
    public Guid HealthRecordId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ApproveAccessRequestRequest
{
    public DateTime AccessStartDateTime { get; set; }
    public DateTime AccessEndDateTime { get; set; }
    public string? ReviewComment { get; set; }
}

public class DeclineAccessRequestRequest
{
    public string ReviewComment { get; set; } = string.Empty;
}

public class AccessRequestResponse
{
    public Guid Id { get; set; }
    public Guid HealthRecordId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; }
    public string RequestingUsername { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public AccessRequestStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByUsername { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? AccessStartDateTime { get; set; }
    public DateTime? AccessEndDateTime { get; set; }
    public bool IsAccessActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
