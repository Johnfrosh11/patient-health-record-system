using PatientHealthRecord.Application.DTOs.Roles;

namespace PatientHealthRecord.Application.Services.Roles;

/// <summary>
/// Role service interface - manages roles and permissions
/// </summary>
public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<RoleResponse> GetByIdAsync(
        Guid roleId, 
        CancellationToken cancellationToken = default);
    
    Task<RoleResponse> CreateAsync(
        CreateRoleRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<RoleResponse> UpdateAsync(
        Guid roleId, 
        UpdateRoleRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<DeleteRoleResponse> DeleteAsync(
        Guid roleId, 
        CancellationToken cancellationToken = default);
    
    Task<RolePermissionResponse> AssignPermissionAsync(
        Guid roleId, 
        Guid permissionId, 
        CancellationToken cancellationToken = default);
    
    Task<RolePermissionResponse> RemovePermissionAsync(
        Guid roleId, 
        Guid permissionId, 
        CancellationToken cancellationToken = default);
    
    Task<List<PermissionResponse>> GetAllPermissionsAsync(
        CancellationToken cancellationToken = default);
}
