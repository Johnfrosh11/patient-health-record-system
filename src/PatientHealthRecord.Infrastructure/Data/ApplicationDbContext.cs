using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using System.Reflection;
using System.Text.Json;

namespace PatientHealthRecord.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<HealthRecord> HealthRecords { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLog>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.Entity is AuditLog)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = currentTime;
                    auditEntries.Add(CreateAuditLog(entry, AuditAction.Create));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = currentTime;
                    auditEntries.Add(CreateAuditLog(entry, AuditAction.Update));
                    break;

                case EntityState.Deleted:
                    auditEntries.Add(CreateAuditLog(entry, AuditAction.Delete));
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Any())
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private AuditLog CreateAuditLog(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, AuditAction action)
    {
        var entityName = entry.Entity.GetType().Name;
        var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

        var auditLog = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            UserId = null,
            Username = null,
            IpAddress = null,
            UserAgent = null
        };

        if (action == AuditAction.Update)
        {
            var changedProperties = new List<string>();
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (IsSensitiveProperty(property.Metadata.Name))
                    continue;

                if (property.IsModified)
                {
                    changedProperties.Add(property.Metadata.Name);
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
            }

            if (changedProperties.Any())
            {
                auditLog.ChangedProperties = string.Join(", ", changedProperties);
                auditLog.OldValues = JsonSerializer.Serialize(oldValues);
                auditLog.NewValues = JsonSerializer.Serialize(newValues);
            }
        }
        else if (action == AuditAction.Create)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                if (!IsSensitiveProperty(property.Metadata.Name))
                {
                    values[property.Metadata.Name] = property.CurrentValue;
                }
            }
            auditLog.NewValues = JsonSerializer.Serialize(values);
        }
        else if (action == AuditAction.Delete)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                if (!IsSensitiveProperty(property.Metadata.Name))
                {
                    values[property.Metadata.Name] = property.CurrentValue;
                }
            }
            auditLog.OldValues = JsonSerializer.Serialize(values);
        }

        return auditLog;
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var sensitiveProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash", "PasswordSalt", "Token", "RefreshToken",
            "Diagnosis", "TreatmentPlan", "MedicalHistory", "Prescription"
        };

        return sensitiveProperties.Contains(propertyName);
    }
}
