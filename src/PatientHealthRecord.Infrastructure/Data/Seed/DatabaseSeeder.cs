using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Infrastructure.Data;
using BCrypt.Net;

namespace PatientHealthRecord.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var permissions = new List<Permission>();
        foreach (var permissionName in Permissions.GetAll())
        {
            permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                Description = GetPermissionDescription(permissionName),
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = Roles.Admin,
            Description = "Administrator with full system access",
            CreatedAt = DateTime.UtcNow
        };

        var doctorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = Roles.Doctor,
            Description = "Doctor with comprehensive patient record access",
            CreatedAt = DateTime.UtcNow
        };

        var nurseRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = Roles.Nurse,
            Description = "Nurse with limited patient record access",
            CreatedAt = DateTime.UtcNow
        };

        var receptionistRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = Roles.Receptionist,
            Description = "Receptionist with minimal access",
            CreatedAt = DateTime.UtcNow
        };

        await context.Roles.AddRangeAsync(new[] { adminRole, doctorRole, nurseRole, receptionistRole });
        await context.SaveChangesAsync();

        var rolePermissions = new List<RolePermission>();

        foreach (var permission in permissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }

        var doctorPermissions = new[]
        {
            Permissions.ViewPatientRecords,
            Permissions.CreatePatientRecords,
            Permissions.UpdatePatientRecords,
            Permissions.DeletePatientRecords,
            Permissions.ApproveAccessRequests
        };
        foreach (var permName in doctorPermissions)
        {
            var perm = permissions.First(p => p.Name == permName);
            rolePermissions.Add(new RolePermission { RoleId = doctorRole.Id, PermissionId = perm.Id });
        }

        var nursePermissions = new[]
        {
            Permissions.CreatePatientRecords,
            Permissions.UpdatePatientRecords,
            Permissions.ApproveAccessRequests
        };
        foreach (var permName in nursePermissions)
        {
            var perm = permissions.First(p => p.Name == permName);
            rolePermissions.Add(new RolePermission { RoleId = nurseRole.Id, PermissionId = perm.Id });
        }

        var receptionistPermissions = new[] { Permissions.CreatePatientRecords };
        foreach (var permName in receptionistPermissions)
        {
            var perm = permissions.First(p => p.Name == permName);
            rolePermissions.Add(new RolePermission { RoleId = receptionistRole.Id, PermissionId = perm.Id });
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@interswitch.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var doctorUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "doctor",
            Email = "doctor@interswitch.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var nurseUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "nurse",
            Email = "nurse@interswitch.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Nurse@123", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var receptionistUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "receptionist",
            Email = "receptionist@interswitch.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Reception@123", workFactor: 12),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(new[] { adminUser, doctorUser, nurseUser, receptionistUser });
        await context.SaveChangesAsync();

        var userRoles = new List<UserRole>
        {
            new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
            new UserRole { UserId = doctorUser.Id, RoleId = doctorRole.Id },
            new UserRole { UserId = nurseUser.Id, RoleId = nurseRole.Id },
            new UserRole { UserId = receptionistUser.Id, RoleId = receptionistRole.Id }
        };

        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();

        var healthRecords = new List<HealthRecord>
        {
            new HealthRecord
            {
                Id = Guid.NewGuid(),
                PatientName = "John Doe",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 5, 15), DateTimeKind.Utc),
                Diagnosis = "Hypertension",
                TreatmentPlan = "Lifestyle modifications and medication: Amlodipine 5mg once daily",
                MedicalHistory = "Family history of cardiovascular disease. Non-smoker.",
                CreatedBy = doctorUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new HealthRecord
            {
                Id = Guid.NewGuid(),
                PatientName = "Jane Smith",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 8, 22), DateTimeKind.Utc),
                Diagnosis = "Type 2 Diabetes Mellitus",
                TreatmentPlan = "Diet control, exercise regimen, Metformin 500mg twice daily",
                MedicalHistory = "Diagnosed 2 years ago. Mother has diabetes.",
                CreatedBy = doctorUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                IsDeleted = false
            },
            new HealthRecord
            {
                Id = Guid.NewGuid(),
                PatientName = "Robert Johnson",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1978, 12, 3), DateTimeKind.Utc),
                Diagnosis = "Chronic Lower Back Pain",
                TreatmentPlan = "Physical therapy, NSAIDs as needed, core strengthening exercises",
                MedicalHistory = "Previous history of lumbar strain. Works in construction.",
                CreatedBy = nurseUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            },
            new HealthRecord
            {
                Id = Guid.NewGuid(),
                PatientName = "Emily Brown",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1995, 3, 18), DateTimeKind.Utc),
                Diagnosis = "Seasonal Allergic Rhinitis",
                TreatmentPlan = "Antihistamine: Loratadine 10mg daily during allergy season, nasal corticosteroid spray",
                MedicalHistory = "Allergies to pollen and dust mites. No previous hospitalizations.",
                CreatedBy = receptionistUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new HealthRecord
            {
                Id = Guid.NewGuid(),
                PatientName = "Michael Wilson",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1982, 7, 29), DateTimeKind.Utc),
                Diagnosis = "Gastroesophageal Reflux Disease (GERD)",
                TreatmentPlan = "Proton pump inhibitor: Omeprazole 20mg before breakfast, dietary modifications",
                MedicalHistory = "Symptoms for 6 months. No previous surgeries.",
                CreatedBy = doctorUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            }
        };

        await context.HealthRecords.AddRangeAsync(healthRecords);
        await context.SaveChangesAsync();

        var accessRequests = new List<AccessRequest>
        {
            new AccessRequest
            {
                Id = Guid.NewGuid(),
                HealthRecordId = healthRecords[0].Id,
                RequestingUserId = receptionistUser.Id,
                Reason = "Need to update patient contact information and schedule follow-up appointment",
                RequestDate = DateTime.UtcNow.AddHours(-2),
                Status = AccessRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new AccessRequest
            {
                Id = Guid.NewGuid(),
                HealthRecordId = healthRecords[1].Id,
                RequestingUserId = receptionistUser.Id,
                Reason = "Patient requested copy of medical records for insurance purposes",
                RequestDate = DateTime.UtcNow.AddDays(-2),
                Status = AccessRequestStatus.Approved,
                ReviewedBy = doctorUser.Id,
                ReviewedDate = DateTime.UtcNow.AddDays(-2).AddHours(1),
                ReviewComment = "Approved for administrative purposes",
                AccessStartDateTime = DateTime.UtcNow.AddDays(-1),
                AccessEndDateTime = DateTime.UtcNow.AddDays(2),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2).AddHours(1)
            },
            new AccessRequest
            {
                Id = Guid.NewGuid(),
                HealthRecordId = healthRecords[2].Id,
                RequestingUserId = receptionistUser.Id,
                Reason = "Curiosity about patient condition",
                RequestDate = DateTime.UtcNow.AddDays(-3),
                Status = AccessRequestStatus.Declined,
                ReviewedBy = nurseUser.Id,
                ReviewedDate = DateTime.UtcNow.AddDays(-3).AddMinutes(30),
                ReviewComment = "Request reason not sufficient for access",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(30)
            }
        };

        await context.AccessRequests.AddRangeAsync(accessRequests);
        await context.SaveChangesAsync();
    }

    private static string GetPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            Permissions.ViewPatientRecords => "View all patient health records in the system",
            Permissions.CreatePatientRecords => "Create new patient health records",
            Permissions.UpdatePatientRecords => "Update existing patient health records (own records only)",
            Permissions.DeletePatientRecords => "Soft delete patient health records (own records only)",
            Permissions.ApproveAccessRequests => "Approve or decline access requests from other users",
            Permissions.ManageUsers => "Create and manage user accounts (admin only)",
            Permissions.ManageRoles => "Create and manage roles and permissions (admin only)",
            _ => string.Empty
        };
    }
}
