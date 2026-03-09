using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Refresh Token entity for JWT token refresh
/// </summary>
public class TRefreshToken : BaseEntity
{
    [Key]
    public Guid     RefreshTokenId { get; set; } = Guid.NewGuid();
    public Guid     UserId         { get; set; }
    public string   Token          { get; set; } = string.Empty;
    public DateTime ExpiresAt      { get; set; }
    public bool     IsRevoked      { get; set; }
    public string?  RevokedReason  { get; set; }

    // Navigation properties
    public TUser User { get; set; } = null!;
}
