using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.EntityMapping;

public sealed class AccessRequestEntityMapping : IEntityTypeConfiguration<TAccessRequest>
{
    public void Configure(EntityTypeBuilder<TAccessRequest> builder)
    {
        builder.ToTable("t_access_request", tb => tb.HasComment("Time-bound access requests"));

        // Primary key
        builder.HasKey(e => e.AccessRequestId);
        builder.Property(e => e.AccessRequestId).HasColumnName("access_request_id");

        // Core columns
        builder.Property(e => e.HealthRecordId).HasColumnName("health_record_id").IsRequired();
        builder.Property(e => e.RequestingUserId).HasColumnName("requesting_user_id").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.RequestDate).HasColumnName("request_date").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").IsRequired();
        builder.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
        builder.Property(e => e.ReviewedDate).HasColumnName("reviewed_date");
        builder.Property(e => e.ReviewComment).HasColumnName("review_comment").HasMaxLength(1000);
        builder.Property(e => e.AccessStartDateTime).HasColumnName("access_start_datetime");
        builder.Property(e => e.AccessEndDateTime).HasColumnName("access_end_datetime");

        // Base entity columns
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.LastModified).HasColumnName("last_modified");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        // Indexes
        builder.HasIndex(e => new { e.OrganizationId, e.IsActive })
               .HasDatabaseName("ix_access_request_org_active");
        builder.HasIndex(e => new { e.HealthRecordId, e.Status })
               .HasDatabaseName("ix_access_request_record_status");
        builder.HasIndex(e => new { e.RequestingUserId, e.Status })
               .HasDatabaseName("ix_access_request_user_status");
        builder.HasIndex(e => e.RequestDate)
               .HasDatabaseName("ix_access_request_date");
    }
}
