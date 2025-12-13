using PatientHealthRecord.Domain.Common;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Domain.Entities;

public class AccessRequest : BaseEntity
{
    public Guid HealthRecordId { get; set; }
    public Guid RequestingUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public AccessRequestStatus Status { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? AccessStartDateTime { get; set; }
    public DateTime? AccessEndDateTime { get; set; }

    public HealthRecord HealthRecord { get; set; } = null!;
    public User RequestingUser { get; set; } = null!;
    public User? Reviewer { get; set; }
}
