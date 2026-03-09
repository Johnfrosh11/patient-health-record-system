namespace PatientHealthRecord.Application.DTOs.Users;

/// <summary>
/// Create User DTO
/// </summary>
public class CreateUserDto
{
    public string Username       { get; set; } = string.Empty;
    public string Email          { get; set; } = string.Empty;
    public string Password       { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public Guid   OrganizationId { get; set; }
    public List<Guid> RoleIds    { get; set; } = [];
}

/// <summary>
/// Update User DTO
/// </summary>
public class UpdateUserDto
{
    public Guid   UserId    { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public bool   IsActive  { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
}

/// <summary>
/// User ViewModel (output)
/// </summary>
public class UserViewModel
{
    public Guid   UserId         { get; set; }
    public string Username       { get; set; } = string.Empty;
    public string Email          { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public string FullName       { get; set; } = string.Empty;
    public Guid   OrganizationId { get; set; }
    public bool   IsActive       { get; set; }
    public string CreatedDate    { get; set; } = string.Empty;
    public List<string> Roles    { get; set; } = [];
}
