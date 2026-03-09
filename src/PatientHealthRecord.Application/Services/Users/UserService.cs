using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Users;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;

namespace PatientHealthRecord.Application.Services.Users;

/// <summary>
/// User service implementation - handles user management operations
/// </summary>
public sealed class UserService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    ILogger<UserService> logger
) : IUserService
{
    public async Task<PaginatedUsersResponse> GetAllAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedUsersResponse
        {
            Items = users.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == id && u.IsActive, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {id} not found.");

        return MapToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate username
        var usernameExists = await db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (usernameExists)
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        // Check for duplicate email
        var emailExists = await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var user = new TUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            FirstName = request.FirstName,
            LastName = request.LastName,
            OrganizationId = authUser.OrganizationId,
            CreatedBy = authUser.UserId.ToString(),
            IpAddress = authUser.IpAddress
        };

        await db.Users.AddAsync(user, cancellationToken);

        // Assign roles if provided
        if (request.RoleIds.Any())
        {
            var roles = await db.Roles
                .Where(r => request.RoleIds.Contains(r.RoleId) && r.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
            {
                user.UserRoles.Add(new TUserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Username} created by {CreatedBy}", user.Username, authUser.UserId);

        // Reload with navigation properties
        await db.Entry(user).Collection(u => u.UserRoles).LoadAsync(cancellationToken);
        foreach (var userRole in user.UserRoles)
        {
            await db.Entry(userRole).Reference(ur => ur.Role).LoadAsync(cancellationToken);
            await db.Entry(userRole.Role).Collection(r => r.RolePermissions).LoadAsync(cancellationToken);
            foreach (var rp in userRole.Role.RolePermissions)
            {
                await db.Entry(rp).Reference(r => r.Permission).LoadAsync(cancellationToken);
            }
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == id && u.IsActive, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await db.Users.AnyAsync(
                u => u.Email == request.Email && u.UserId != id, 
                cancellationToken);
            if (emailExists)
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.LastModified = DateTime.UtcNow;
        user.ModifiedBy = authUser.UserId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} updated by {ModifiedBy}", user.UserId, authUser.UserId);

        return MapToResponse(user);
    }

    public async Task<UserResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id && u.IsActive, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {id} not found.");

        if (user.UserId == authUser.UserId)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        user.IsActive = false;
        user.LastModified = DateTime.UtcNow;
        user.ModifiedBy = authUser.UserId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} deactivated by {ModifiedBy}", user.UserId, authUser.UserId);

        return MapToResponse(user);
    }

    public async Task<UserRoleResponse> AssignRoleAsync(
        Guid userId, 
        Guid roleId, 
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found.");

        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsActive, cancellationToken);

        if (role == null)
            throw new KeyNotFoundException($"Role {roleId} not found.");

        // Check if already assigned
        if (user.UserRoles.Any(ur => ur.RoleId == roleId))
            throw new InvalidOperationException($"Role '{role.RoleName}' is already assigned to user '{user.Username}'.");

        await db.UserRoles.AddAsync(new TUserRole
        {
            UserId = userId,
            RoleId = roleId
        }, cancellationToken);

        user.LastModified = DateTime.UtcNow;
        user.ModifiedBy = authUser.UserId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Role {RoleName} assigned to user {Username} by {AssignedBy}",
            role.RoleName, user.Username, authUser.UserId);

        return new UserRoleResponse
        {
            UserId = userId,
            Username = user.Username,
            RoleId = roleId,
            RoleName = role.RoleName,
            Message = $"Role '{role.RoleName}' assigned to user '{user.Username}'."
        };
    }

    public async Task<UserRoleResponse> RemoveRoleAsync(
        Guid userId, 
        Guid roleId, 
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found.");

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            throw new KeyNotFoundException($"User does not have this role.");

        var role = await db.Roles.FindAsync(roleId, cancellationToken);

        db.UserRoles.Remove(userRole);

        user.LastModified = DateTime.UtcNow;
        user.ModifiedBy = authUser.UserId.ToString();

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Role {RoleName} removed from user {Username} by {RemovedBy}",
            role?.RoleName, user.Username, authUser.UserId);

        return new UserRoleResponse
        {
            UserId = userId,
            Username = user.Username,
            RoleId = roleId,
            RoleName = role?.RoleName ?? string.Empty,
            Message = $"Role removed from user '{user.Username}'."
        };
    }

    private static UserResponse MapToResponse(TUser user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionName)
            .Distinct()
            .ToList();

        return new UserResponse
        {
            Id = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roles,
            Permissions = permissions,
            CreatedAt = user.CreatedDate,
            LastModifiedDate = user.LastModified
        };
    }
}
