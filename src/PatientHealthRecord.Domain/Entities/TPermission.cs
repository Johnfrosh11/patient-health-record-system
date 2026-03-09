using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Permission entity
/// </summary>
public class TPermission : BaseEntityNoOrg  // System table
{
    [Key]
    public Guid           PermissionId   { get; set; } = Guid.NewGuid();
    public string         PermissionName { get; set; } = string.Empty;
    public string         Description    { get; set; } = string.Empty;
    public PermissionType Type           { get; set; } = PermissionType.Unknown;

    // Navigation properties
    public ICollection<TRolePermission> RolePermissions { get; set; } = [];
}
