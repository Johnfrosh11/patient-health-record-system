using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Roles;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Roles controller - role and permission management (requires manageRoles permission)
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Policy = Permissions.ManageRoles)]
public sealed class RolesController(IRoleService svc) : BaseController
{
    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await svc.GetAllAsync(ct));

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(await svc.GetByIdAsync(id, ct));

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct = default)
        => Ok(await svc.CreateAsync(request, ct));

    /// <summary>
    /// Update role details
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct = default)
        => Ok(await svc.UpdateAsync(id, request, ct));

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        => Ok(await svc.DeleteAsync(id, ct));

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions(CancellationToken ct = default)
        => Ok(await svc.GetAllPermissionsAsync(ct));

    /// <summary>
    /// Assign a permission to a role
    /// </summary>
    [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> AssignPermission(Guid roleId, Guid permissionId, CancellationToken ct = default)
        => Ok(await svc.AssignPermissionAsync(roleId, permissionId, ct));

    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemovePermission(Guid roleId, Guid permissionId, CancellationToken ct = default)
        => Ok(await svc.RemovePermissionAsync(roleId, permissionId, ct));
}
