using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Role-Permission junction table
/// </summary>
public class TRolePermission
{
    [Key]
    public Guid RolePermissionId { get; set; } = Guid.NewGuid();
    public Guid RoleId           { get; set; }
    public Guid PermissionId     { get; set; }

    // Navigation properties
    public TRole       Role       { get; set; } = null!;
    public TPermission Permission { get; set; } = null!;
}
