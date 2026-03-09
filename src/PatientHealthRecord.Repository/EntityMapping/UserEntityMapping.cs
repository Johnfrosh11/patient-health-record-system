using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.EntityMapping;

public sealed class UserEntityMapping : IEntityTypeConfiguration<TUser>
{
    public void Configure(EntityTypeBuilder<TUser> builder)
    {
        builder.ToTable("t_user", tb => tb.HasComment("System users"));

        // Primary key
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserId).HasColumnName("user_id");

        // Core columns (snake_case)
        builder.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        
        // Base entity columns
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.LastModified).HasColumnName("last_modified");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.IpAddress).HasColumnName("ip_address");

        // Indexes — all fields used in WHERE/JOIN/ORDER BY must be indexed
        builder.HasIndex(e => new { e.OrganizationId, e.IsActive })
               .HasDatabaseName("ix_user_org_active");
        builder.HasIndex(e => e.Email)
               .IsUnique()
               .HasDatabaseName("ix_user_email_unique");
        builder.HasIndex(e => e.Username)
               .IsUnique()
               .HasDatabaseName("ix_user_username_unique");
        builder.HasIndex(e => e.CreatedDate)
               .HasDatabaseName("ix_user_created_date");

        // Relationships
        builder.HasMany(e => e.CreatedHealthRecords)
               .WithOne(h => h.Creator)
               .HasForeignKey(h => h.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AccessRequests)
               .WithOne(a => a.RequestingUser)
               .HasForeignKey(a => a.RequestingUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ReviewedAccessRequests)
               .WithOne(a => a.Reviewer)
               .HasForeignKey(a => a.ReviewedBy)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
