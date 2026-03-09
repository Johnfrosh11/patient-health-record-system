using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;

namespace PatientHealthRecord.Repository.Seed;

/// <summary>
/// Database seeder for initial system data
/// </summary>
public static class DatabaseSeeder
{
    private static readonly Guid TestOrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PatientHealthRecordDbContext>();

        // Seed Permissions (using correct names from AuthConstants)
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new List<TPermission>
            {
                new() { PermissionName = Permissions.ViewPatientRecords, Description = "View patient records", CreatedBy = "system" },
                new() { PermissionName = Permissions.CreatePatientRecords, Description = "Create patient records", CreatedBy = "system" },
                new() { PermissionName = Permissions.UpdatePatientRecords, Description = "Update patient records", CreatedBy = "system" },
                new() { PermissionName = Permissions.DeletePatientRecords, Description = "Delete patient records", CreatedBy = "system" },
                new() { PermissionName = Permissions.ApproveAccessRequests, Description = "Approve access requests", CreatedBy = "system" },
                new() { PermissionName = Permissions.ManageUsers, Description = "Manage users", CreatedBy = "system" },
                new() { PermissionName = Permissions.ManageRoles, Description = "Manage roles", CreatedBy = "system" },
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<TRole>
            {
                new() { RoleName = "Admin", Description = "System administrator with full access", CreatedBy = "system" },
                new() { RoleName = "Doctor", Description = "Medical professional who can create and update records", CreatedBy = "system" },
                new() { RoleName = "Nurse", Description = "Healthcare worker with limited access", CreatedBy = "system" },
                new() { RoleName = "Patient", Description = "Patient with access to own records", CreatedBy = "system" },
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Seed Role-Permission mappings
            var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
            var doctorRole = await context.Roles.FirstAsync(r => r.RoleName == "Doctor");
            var allPermissions = await context.Permissions.ToListAsync();

            var rolePermissions = new List<TRolePermission>();

            // Admin gets all permissions
            foreach (var permission in allPermissions)
            {
                rolePermissions.Add(new TRolePermission
                {
                    RoleId = adminRole.RoleId,
                    PermissionId = permission.PermissionId
                });
            }

            // Doctor gets view, create, update, and approve permissions
            var doctorPermissionNames = new[] { 
                Permissions.ViewPatientRecords, 
                Permissions.CreatePatientRecords, 
                Permissions.UpdatePatientRecords, 
                Permissions.ApproveAccessRequests 
            };
            foreach (var permission in allPermissions.Where(p => doctorPermissionNames.Contains(p.PermissionName)))
            {
                rolePermissions.Add(new TRolePermission
                {
                    RoleId = doctorRole.RoleId,
                    PermissionId = permission.PermissionId
                });
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");

            var adminUser = new TUser
            {
                UserId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Username = "admin",
                Email = "admin@testhospital.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                OrganizationId = TestOrgId,
                IsActive = true,
                CreatedBy = "system"
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            // Assign Admin role to admin user
            await context.UserRoles.AddAsync(new TUserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId
            });

            await context.SaveChangesAsync();
        }
    }
}
