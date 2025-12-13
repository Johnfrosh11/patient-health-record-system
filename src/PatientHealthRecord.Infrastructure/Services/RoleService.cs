using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Roles;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;

namespace PatientHealthRecord.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<RoleService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Permissions = r.RolePermissions.Select(rp => new PermissionResponse
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description
            }).ToList(),
            UserCount = r.UserRoles.Count,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), id);
        }

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.RolePermissions.Select(rp => new PermissionResponse
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description
            }).ToList(),
            UserCount = role.UserRoles.Count,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingRole != null)
        {
            throw new ValidationException("Role name already exists");
        }

        var permissions = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (permissions.Count != request.PermissionIds.Count)
        {
            throw new ValidationException("One or more permission IDs are invalid");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            var rolePermission = new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            };
            _context.RolePermissions.Add(rolePermission);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleName} created", role.Name);

        return await GetByIdAsync(role.Id, cancellationToken);
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FindAsync(new object[] { id }, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), id);
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == request.Name.ToLower() && r.Id != id, cancellationToken);

            if (existingRole != null)
            {
                throw new ValidationException("Role name already exists");
            }

            role.Name = request.Name;
        }

        if (request.Description != null)
        {
            role.Description = request.Description;
        }

        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} updated", id);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<DeleteRoleResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), id);
        }

        if (role.UserRoles.Any())
        {
            throw new ValidationException($"Cannot delete role '{role.Name}' because it has {role.UserRoles.Count} user(s) assigned. Remove all users from this role before deleting.");
        }

        var roleName = role.Name;
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} deleted", id);

        return new DeleteRoleResponse
        {
            Id = id,
            Name = roleName,
            DeletedAt = DateTime.UtcNow,
            Message = $"Role '{roleName}' has been deleted successfully"
        };
    }

    public async Task<RolePermissionResponse> AssignPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), roleId);
        }

        var permission = await _context.Permissions.FindAsync(new object[] { permissionId }, cancellationToken);

        if (permission == null)
        {
            throw new NotFoundException(nameof(Permission), permissionId);
        }

        if (role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
        {
            throw new ValidationException("Permission already assigned to role");
        }

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", permissionId, roleId);

        return new RolePermissionResponse
        {
            RoleId = roleId,
            RoleName = role.Name,
            PermissionId = permissionId,
            PermissionName = permission.Name,
            Message = $"Permission '{permission.Name}' has been assigned to role '{role.Name}'"
        };
    }

    public async Task<RolePermissionResponse> RemovePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission == null)
        {
            throw new NotFoundException("Role permission assignment not found");
        }

        var permissionCount = await _context.RolePermissions
            .CountAsync(rp => rp.RoleId == roleId, cancellationToken);

        if (permissionCount <= 1)
        {
            throw new ValidationException("Cannot remove the last permission from a role. Roles must have at least one permission");
        }

        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
        var permission = await _context.Permissions.FindAsync(new object[] { permissionId }, cancellationToken);

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);

        return new RolePermissionResponse
        {
            RoleId = roleId,
            RoleName = role?.Name ?? string.Empty,
            PermissionId = permissionId,
            PermissionName = permission?.Name ?? string.Empty,
            Message = $"Permission '{permission?.Name}' has been removed from role '{role?.Name}'"
        };
    }

    public async Task<List<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return permissions.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToList();
    }
}
