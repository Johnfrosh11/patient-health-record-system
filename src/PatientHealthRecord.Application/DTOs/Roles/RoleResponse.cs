namespace PatientHealthRecord.Application.DTOs.Roles;

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PermissionResponse> Permissions { get; set; } = new();
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
