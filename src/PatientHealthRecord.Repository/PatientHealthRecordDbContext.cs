using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository;

/// <summary>
/// PatientHealthRecord DbContext - main data access context
/// </summary>
public sealed class PatientHealthRecordDbContext(DbContextOptions<PatientHealthRecordDbContext> options)
    : DbContext(options)
{
    // DbSet naming: entity name without T prefix
    public DbSet<TUser>           Users           { get; set; }
    public DbSet<TRole>           Roles           { get; set; }
    public DbSet<TPermission>     Permissions     { get; set; }
    public DbSet<TUserRole>       UserRoles       { get; set; }
    public DbSet<TRolePermission> RolePermissions { get; set; }
    public DbSet<THealthRecord>   HealthRecords   { get; set; }
    public DbSet<TAccessRequest>  AccessRequests  { get; set; }
    public DbSet<TRefreshToken>   RefreshTokens   { get; set; }
    public DbSet<TAuditLog>       AuditLogs       { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Auto-applies all IEntityTypeConfiguration<T> in this assembly
        mb.ApplyConfigurationsFromAssembly(typeof(PatientHealthRecordDbContext).Assembly);
        base.OnModelCreating(mb);
    }
}
