using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// User-Role junction table
/// </summary>
public class TUserRole
{
    [Key]
    public Guid UserRoleId { get; set; } = Guid.NewGuid();
    public Guid UserId     { get; set; }
    public Guid RoleId     { get; set; }

    // Navigation properties
    public TUser User { get; set; } = null!;
    public TRole Role { get; set; } = null!;
}
