using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Audit Log entity for tracking all system actions
/// </summary>
public class TAuditLog : BaseEntity
{
    [Key]
    public Guid       AuditLogId  { get; set; } = Guid.NewGuid();
    public Guid?      UserId      { get; set; }
    public AuditAction Action     { get; set; } = AuditAction.Unknown;
    public string     EntityName  { get; set; } = string.Empty;
    public Guid?      EntityId    { get; set; }
    public string?    OldValues   { get; set; }
    public string?    NewValues   { get; set; }
    public string?    Description { get; set; }
    public DateTime   Timestamp   { get; set; } = DateTime.UtcNow;
}
