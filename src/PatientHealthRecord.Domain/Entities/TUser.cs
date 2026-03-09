using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// User entity (prefixed with T per architecture standard)
/// </summary>
public class TUser : BaseEntity
{
    [Key]
    public Guid   UserId       { get; set; } = Guid.NewGuid();
    public string Username     { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName    { get; set; } = string.Empty;
    public string LastName     { get; set; } = string.Empty;
    
    // xmin concurrency token handled by Npgsql at the mapping layer (UseXminAsConcurrencyToken)

    // Navigation properties
    public ICollection<TUserRole>         UserRoles                { get; set; } = [];
    public ICollection<THealthRecord>     CreatedHealthRecords     { get; set; } = [];
    public ICollection<TRefreshToken>     RefreshTokens            { get; set; } = [];
    public ICollection<TAccessRequest>    AccessRequests           { get; set; } = [];
    public ICollection<TAccessRequest>    ReviewedAccessRequests   { get; set; } = [];
}
