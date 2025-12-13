namespace PatientHealthRecord.Application.DTOs.Users;

public class UserRoleResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
