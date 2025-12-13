namespace PatientHealthRecord.Application.DTOs.Roles;

public class DeleteRoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RolePermissionResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
