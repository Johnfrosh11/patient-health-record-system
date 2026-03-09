using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.EntityMapping;

public sealed class HealthRecordEntityMapping : IEntityTypeConfiguration<THealthRecord>
{
    public void Configure(EntityTypeBuilder<THealthRecord> builder)
    {
        builder.ToTable("t_health_record", tb => tb.HasComment("Patient health records"));

        // Primary key
        builder.HasKey(e => e.HealthRecordId);
        builder.Property(e => e.HealthRecordId).HasColumnName("health_record_id");

        // Core columns
        builder.Property(e => e.PatientName).HasColumnName("patient_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Property(e => e.Diagnosis).HasColumnName("diagnosis").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.TreatmentPlan).HasColumnName("treatment_plan").HasMaxLength(2000).IsRequired();
        builder.Property(e => e.MedicalHistory).HasColumnName("medical_history").HasMaxLength(4000);
        builder.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();

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
               .HasDatabaseName("ix_health_record_org_active");
        builder.HasIndex(e => e.PatientName)
               .HasDatabaseName("ix_health_record_patient_name");
        builder.HasIndex(e => e.CreatedDate)
               .HasDatabaseName("ix_health_record_created_date");
        builder.HasIndex(e => e.CreatedByUserId)
               .HasDatabaseName("ix_health_record_created_by");
    }
}
