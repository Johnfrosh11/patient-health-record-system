using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.EntityMapping;

public sealed class RoleEntityMapping : IEntityTypeConfiguration<TRole>
{
    public void Configure(EntityTypeBuilder<TRole> builder)
    {
        builder.ToTable("t_role", tb => tb.HasComment("System roles"));

        // Primary key
        builder.HasKey(e => e.RoleId);
        builder.Property(e => e.RoleId).HasColumnName("role_id");

        // Core columns
        builder.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

        // Base entity columns (no OrganizationId - system table)
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");

        // Indexes
        builder.HasIndex(e => e.RoleName)
               .IsUnique()
               .HasDatabaseName("ix_role_name_unique");
    }
}

public sealed class PermissionEntityMapping : IEntityTypeConfiguration<TPermission>
{
    public void Configure(EntityTypeBuilder<TPermission> builder)
    {
        builder.ToTable("t_permission", tb => tb.HasComment("System permissions"));

        // Primary key
        builder.HasKey(e => e.PermissionId);
        builder.Property(e => e.PermissionId).HasColumnName("permission_id");

        // Core columns
        builder.Property(e => e.PermissionName).HasColumnName("permission_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(e => e.Type).HasColumnName("type").IsRequired();

        // Base entity columns
        builder.Property(e => e.CreatedDate).HasColumnName("created_date");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.IsActive).HasColumnName("is_active");

        // Indexes
        builder.HasIndex(e => e.PermissionName)
               .IsUnique()
               .HasDatabaseName("ix_permission_name_unique");
    }
}

public sealed class UserRoleEntityMapping : IEntityTypeConfiguration<TUserRole>
{
    public void Configure(EntityTypeBuilder<TUserRole> builder)
    {
        builder.ToTable("t_user_role", tb => tb.HasComment("User-Role junction table"));

        // Primary key
        builder.HasKey(e => e.UserRoleId);
        builder.Property(e => e.UserRoleId).HasColumnName("user_role_id");

        // Foreign keys
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.RoleId })
               .IsUnique()
               .HasDatabaseName("ix_user_role_unique");

        // Relationships
        builder.HasOne(e => e.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(e => e.RoleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RolePermissionEntityMapping : IEntityTypeConfiguration<TRolePermission>
{
    public void Configure(EntityTypeBuilder<TRolePermission> builder)
    {
        builder.ToTable("t_role_permission", tb => tb.HasComment("Role-Permission junction table"));

        // Primary key
        builder.HasKey(e => e.RolePermissionId);
        builder.Property(e => e.RolePermissionId).HasColumnName("role_permission_id");

        // Foreign keys
        builder.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(e => e.PermissionId).HasColumnName("permission_id").IsRequired();

        // Indexes
        builder.HasIndex(e => new { e.RoleId, e.PermissionId })
               .IsUnique()
               .HasDatabaseName("ix_role_permission_unique");

        // Relationships
        builder.HasOne(e => e.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(e => e.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permission)
               .WithMany(p => p.RolePermissions)
               .HasForeignKey(e => e.PermissionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
