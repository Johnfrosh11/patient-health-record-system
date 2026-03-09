using PatientHealthRecord.Application.DTOs.Users;

namespace PatientHealthRecord.Application.Services.Users;

/// <summary>
/// User service interface - manages users and role assignments
/// </summary>
public interface IUserService
{
    Task<PaginatedUsersResponse> GetAllAsync(
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default);
    
    Task<UserResponse> GetByIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<UserResponse> CreateAsync(
        CreateUserRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<UserResponse> UpdateAsync(
        Guid userId, 
        UpdateUserRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<UserResponse> DeactivateAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<UserRoleResponse> AssignRoleAsync(
        Guid userId, 
        Guid roleId, 
        CancellationToken cancellationToken = default);
    
    Task<UserRoleResponse> RemoveRoleAsync(
        Guid userId, 
        Guid roleId, 
        CancellationToken cancellationToken = default);
}
