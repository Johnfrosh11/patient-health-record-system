using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Access Request entity - time-bound access requests to view health records
/// </summary>
public class TAccessRequest : BaseEntity
{
    [Key]
    public Guid     AccessRequestId    { get; set; } = Guid.NewGuid();
    public Guid     HealthRecordId     { get; set; }
    public Guid     RequestingUserId   { get; set; }
    public string   Reason             { get; set; } = string.Empty;
    public DateTime RequestDate        { get; set; } = DateTime.UtcNow;
    public AccessRequestStatus Status  { get; set; } = AccessRequestStatus.Pending;
    public Guid?    ReviewedBy         { get; set; }
    public DateTime? ReviewedDate      { get; set; }
    public string?  ReviewComment      { get; set; }
    public DateTime? AccessStartDateTime { get; set; }
    public DateTime? AccessEndDateTime   { get; set; }

    // Navigation properties
    public THealthRecord HealthRecord    { get; set; } = null!;
    public TUser         RequestingUser  { get; set; } = null!;
    public TUser?        Reviewer        { get; set; }
}
