using PatientHealthRecord.Domain.Common;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Domain.Entities;

public class AuditLog : BaseEntity
{
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedProperties { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public User? User { get; set; }
}
