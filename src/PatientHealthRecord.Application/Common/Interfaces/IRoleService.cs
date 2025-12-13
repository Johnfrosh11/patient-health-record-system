using PatientHealthRecord.Application.DTOs.Roles;

namespace PatientHealthRecord.Application.Common.Interfaces;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<DeleteRoleResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RolePermissionResponse> AssignPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<RolePermissionResponse> RemovePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<List<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
