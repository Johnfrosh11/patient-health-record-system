using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Users;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;

namespace PatientHealthRecord.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedUsersResponse> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .OrderBy(u => u.Username);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userResponses = users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = u.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList(),
            CreatedAt = u.CreatedAt,
            LastModifiedDate = u.UpdatedAt
        }).ToList();

        return new PaginatedUsersResponse
        {
            Items = userResponses,
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
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList(),
            CreatedAt = user.CreatedAt,
            LastModifiedDate = user.UpdatedAt
        };
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (existingUser != null)
        {
            throw new ValidationException("Username already exists");
        }

        var existingEmail = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingEmail != null)
        {
            throw new ValidationException("Email already exists");
        }

        var roles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (roles.Count != request.RoleIds.Count)
        {
            throw new ValidationException("One or more role IDs are invalid");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var role in roles)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            _context.UserRoles.Add(userRole);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} created by admin", user.Username);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id, cancellationToken);

            if (existingEmail != null)
            {
                throw new ValidationException("Email already exists");
            }

            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated", id);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<UserResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deactivated", id);

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList(),
            CreatedAt = user.CreatedAt,
            LastModifiedDate = user.UpdatedAt
        };
    }

    public async Task<UserRoleResponse> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException(nameof(Role), roleId);
        }

        if (user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            throw new ValidationException("Role already assigned to user");
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} assigned to user {UserId}", roleId, userId);

        return new UserRoleResponse
        {
            UserId = userId,
            Username = user.Username,
            RoleId = roleId,
            RoleName = role.Name,
            Message = $"Role '{role.Name}' has been assigned to user '{user.Username}'"
        };
    }

    public async Task<UserRoleResponse> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (userRole == null)
        {
            throw new NotFoundException("User role assignment not found");
        }

        var roleCount = await _context.UserRoles
            .CountAsync(ur => ur.UserId == userId, cancellationToken);

        if (roleCount <= 1)
        {
            throw new ValidationException("Cannot remove the last role from a user. Users must have at least one role");
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);

        return new UserRoleResponse
        {
            UserId = userId,
            Username = user?.Username ?? string.Empty,
            RoleId = roleId,
            RoleName = role?.Name ?? string.Empty,
            Message = $"Role '{role?.Name}' has been removed from user '{user?.Username}'"
        };
    }
}
