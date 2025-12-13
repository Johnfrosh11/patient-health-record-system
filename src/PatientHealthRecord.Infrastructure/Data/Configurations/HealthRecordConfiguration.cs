using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Infrastructure.Data.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("HealthRecords");

        builder.HasKey(hr => hr.Id);

        builder.Property(hr => hr.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hr => hr.DateOfBirth)
            .IsRequired();

        builder.Property(hr => hr.Diagnosis)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(hr => hr.TreatmentPlan)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(hr => hr.MedicalHistory)
            .HasMaxLength(5000);

        builder.Property(hr => hr.CreatedBy)
            .IsRequired();

        builder.Property(hr => hr.CreatedAt)
            .IsRequired();

        builder.Property(hr => hr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(hr => hr.CreatedBy);
        builder.HasIndex(hr => hr.IsDeleted);
        builder.HasIndex(hr => hr.PatientName);

        builder.HasMany(hr => hr.AccessRequests)
            .WithOne(ar => ar.HealthRecord)
            .HasForeignKey(ar => ar.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
