using PatientHealthRecord.Application.DTOs.Users;

namespace PatientHealthRecord.Application.Common.Interfaces;

public interface IUserService
{
    Task<PaginatedUsersResponse> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserRoleResponse> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<UserRoleResponse> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
