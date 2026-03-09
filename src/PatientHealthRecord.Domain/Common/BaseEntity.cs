namespace PatientHealthRecord.Domain.Common;

/// <summary>
/// Base entity for all domain entities with multi-tenancy support via OrganizationId
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        CreatedDate = DateTime.UtcNow;   // ALWAYS UtcNow — never DateTime.Now
        IsActive    = true;
    }

    public DateTime  CreatedDate    { get; set; }   // Required — auto-set in constructor
    public string    CreatedBy      { get; set; } = string.Empty;  // Required — UserId from JWT
    public DateTime? LastModified   { get; set; }   // Optional — set on updates
    public string?   ModifiedBy     { get; set; }   // Optional — UserId from JWT
    public bool      IsActive       { get; set; }   // Soft delete flag
    public string?   IpAddress      { get; set; }   // Requestor IP
    public Guid      OrganizationId { get; set; }   // ISOLATION KEY — filter on every query
}

/// <summary>
/// Base entity for system/reference tables with no org isolation (e.g. countries, currencies)
/// </summary>
public abstract class BaseEntityNoOrg
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string   CreatedBy   { get; set; } = string.Empty;
    public bool     IsActive    { get; set; } = true;
}
