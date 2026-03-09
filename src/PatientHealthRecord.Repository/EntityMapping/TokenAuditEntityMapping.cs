using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.EntityMapping;

public sealed class RefreshTokenEntityMapping : IEntityTypeConfiguration<TRefreshToken>
{
    public void Configure(EntityTypeBuilder<TRefreshToken> builder)
    {
        builder.ToTable("t_refresh_token", tb => tb.HasComment("JWT refresh tokens"));

        // Primary key
        builder.HasKey(e => e.RefreshTokenId);
        builder.Property(e => e.RefreshTokenId).HasColumnName("refresh_token_id");

        // Core columns
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.IsRevoked).HasColumnName("is_revoked").IsRequired();
        builder.Property(e => e.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(500);

        // Base entity columns
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.LastModified).HasColumnName("last_modified");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        // Indexes
        builder.HasIndex(e => e.Token)
               .IsUnique()
               .HasDatabaseName("ix_refresh_token_token_unique");
        builder.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt })
               .HasDatabaseName("ix_refresh_token_user_valid");

        // Relationships
        builder.HasOne(e => e.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AuditLogEntityMapping : IEntityTypeConfiguration<TAuditLog>
{
    public void Configure(EntityTypeBuilder<TAuditLog> builder)
    {
        builder.ToTable("t_audit_log", tb => tb.HasComment("System audit logs"));

        // Primary key
        builder.HasKey(e => e.AuditLogId);
        builder.Property(e => e.AuditLogId).HasColumnName("audit_log_id");

        // Core columns
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Action).HasColumnName("action").IsRequired();
        builder.Property(e => e.EntityName).HasColumnName("entity_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityId).HasColumnName("entity_id");
        builder.Property(e => e.OldValues).HasColumnName("old_values");
        builder.Property(e => e.NewValues).HasColumnName("new_values");
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();

        // Base entity columns
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.LastModified).HasColumnName("last_modified");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        // Indexes
        builder.HasIndex(e => new { e.OrganizationId, e.Timestamp })
               .HasDatabaseName("ix_audit_log_org_timestamp");
        builder.HasIndex(e => new { e.UserId, e.Action })
               .HasDatabaseName("ix_audit_log_user_action");
        builder.HasIndex(e => new { e.EntityName, e.EntityId })
               .HasDatabaseName("ix_audit_log_entity");
    }
}
