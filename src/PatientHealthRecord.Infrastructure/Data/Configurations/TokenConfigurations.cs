using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Infrastructure.Data.Configurations;

public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("AccessRequests");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.HealthRecordId)
            .IsRequired();

        builder.Property(ar => ar.RequestingUserId)
            .IsRequired();

        builder.Property(ar => ar.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ar => ar.RequestDate)
            .IsRequired();

        builder.Property(ar => ar.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ar => ar.ReviewComment)
            .HasMaxLength(500);

        builder.Property(ar => ar.CreatedAt)
            .IsRequired();

        builder.HasIndex(ar => ar.HealthRecordId);
        builder.HasIndex(ar => ar.RequestingUserId);
        builder.HasIndex(ar => ar.Status);
        builder.HasIndex(ar => ar.ReviewedBy);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
