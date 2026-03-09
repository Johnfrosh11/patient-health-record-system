using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Role entity for RBAC
/// </summary>
public class TRole : BaseEntityNoOrg  // System/reference table - no org isolation
{
    [Key]
    public Guid   RoleId      { get; set; } = Guid.NewGuid();
    public string RoleName    { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<TUserRole>       UserRoles       { get; set; } = [];
    public ICollection<TRolePermission> RolePermissions { get; set; } = [];
}
