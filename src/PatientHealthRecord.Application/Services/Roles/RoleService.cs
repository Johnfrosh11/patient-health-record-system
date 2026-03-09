using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Roles;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;

namespace PatientHealthRecord.Application.Services.Roles;

/// <summary>
/// Role service implementation - handles role and permission management
/// </summary>
public sealed class RoleService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    ILogger<RoleService> logger
) : IRoleService
{
    public async Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync(cancellationToken);

        return roles.Select(MapToResponse).ToList();
    }

    public async Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == id && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {id} not found.");

        return MapToResponse(role);
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        var exists = await db.Roles.AnyAsync(r => r.RoleName == request.Name && r.IsActive, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Role '{request.Name}' already exists.");

        var role = new TRole
        {
            RoleName = request.Name,
            Description = request.Description ?? string.Empty,
            CreatedBy = authUser.UserId.ToString(),
            CreatedDate = DateTime.UtcNow
        };

        await db.Roles.AddAsync(role, cancellationToken);

        // Assign permissions if provided
        if (request.PermissionIds.Any())
        {
            var permissions = await db.Permissions
                .Where(p => request.PermissionIds.Contains(p.PermissionId) && p.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                role.RolePermissions.Add(new TRolePermission
                {
                    RoleId = role.RoleId,
                    PermissionId = permission.PermissionId
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Role {RoleName} created by {UserId}", role.RoleName, authUser.UserId);

        return MapToResponse(role);
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == id && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            // Check for duplicate name (excluding current role)
            var exists = await db.Roles.AnyAsync(
                r => r.RoleName == request.Name && r.RoleId != id && r.IsActive, 
                cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Role '{request.Name}' already exists.");

            role.RoleName = request.Name;
        }

        if (request.Description != null)
            role.Description = request.Description;

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Role {RoleId} updated by {UserId}", role.RoleId, authUser.UserId);

        return MapToResponse(role);
    }

    public async Task<DeleteRoleResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == id && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {id} not found.");

        if (role.UserRoles.Any())
            throw new InvalidOperationException($"Cannot delete role '{role.RoleName}' - it is assigned to {role.UserRoles.Count} user(s).");

        // Soft delete
        role.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Role {RoleName} deleted by {UserId}", role.RoleName, authUser.UserId);

        return new DeleteRoleResponse
        {
            Id = role.RoleId,
            Name = role.RoleName,
            DeletedAt = DateTime.UtcNow,
            Message = $"Role '{role.RoleName}' has been deleted."
        };
    }

    public async Task<RolePermissionResponse> AssignPermissionAsync(
        Guid roleId, 
        Guid permissionId, 
        CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {roleId} not found.");

        var permission = await db.Permissions
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId && p.IsActive, cancellationToken);

        if (permission == null)
            throw new KeyNotFoundException($"Permission {permissionId} not found.");

        // Check if already assigned
        if (role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
            throw new InvalidOperationException($"Permission '{permission.PermissionName}' is already assigned to role '{role.RoleName}'.");

        await db.RolePermissions.AddAsync(new TRolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Permission {PermissionName} assigned to role {RoleName} by {UserId}",
            permission.PermissionName, role.RoleName, authUser.UserId);

        return new RolePermissionResponse
        {
            RoleId = roleId,
            RoleName = role.RoleName,
            PermissionId = permissionId,
            PermissionName = permission.PermissionName,
            Message = $"Permission '{permission.PermissionName}' assigned to role '{role.RoleName}'."
        };
    }

    public async Task<RolePermissionResponse> RemovePermissionAsync(
        Guid roleId, 
        Guid permissionId, 
        CancellationToken cancellationToken = default)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {roleId} not found.");

        var rolePermission = role.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission == null)
            throw new KeyNotFoundException($"Permission {permissionId} is not assigned to this role.");

        var permission = await db.Permissions.FindAsync(permissionId, cancellationToken);

        db.RolePermissions.Remove(rolePermission);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Permission {PermissionName} removed from role {RoleName} by {UserId}",
            permission?.PermissionName, role.RoleName, authUser.UserId);

        return new RolePermissionResponse
        {
            RoleId = roleId,
            RoleName = role.RoleName,
            PermissionId = permissionId,
            PermissionName = permission?.PermissionName ?? string.Empty,
            Message = $"Permission removed from role '{role.RoleName}'."
        };
    }

    public async Task<List<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await db.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.PermissionName)
            .ToListAsync(cancellationToken);

        return permissions.Select(p => new PermissionResponse
        {
            Id = p.PermissionId,
            Name = p.PermissionName,
            Description = p.Description
        }).ToList();
    }

    private static RoleResponse MapToResponse(TRole role)
    {
        return new RoleResponse
        {
            Id = role.RoleId,
            Name = role.RoleName,
            Description = role.Description,
            Permissions = role.RolePermissions.Select(rp => new PermissionResponse
            {
                Id = rp.Permission.PermissionId,
                Name = rp.Permission.PermissionName,
                Description = rp.Permission.Description
            }).ToList(),
            UserCount = role.UserRoles?.Count ?? 0,
            CreatedAt = role.CreatedDate
        };
    }
}
