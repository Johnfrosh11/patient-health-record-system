using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Roles;
using PatientHealthRecord.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace PatientHealthRecord.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all roles")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var roles = await _roleService.GetAllAsync(cancellationToken);
        return Response(roles);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get role by ID")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleService.GetByIdAsync(id, cancellationToken);
        return Response(role);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new role")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update role information")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleService.UpdateAsync(id, request, cancellationToken);
        return Response(role);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a role")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        return Response(result);
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    [SwaggerOperation(Summary = "Assign permission to role")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> AssignPermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.AssignPermissionAsync(roleId, permissionId, cancellationToken);
        return Response(result);
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [SwaggerOperation(Summary = "Remove permission from role")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> RemovePermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.RemovePermissionAsync(roleId, permissionId, cancellationToken);
        return Response(result);
    }

    [HttpGet("permissions")]
    [SwaggerOperation(Summary = "Get all available permissions")]
    [Authorize(Policy = Permissions.ManageRoles)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken = default)
    {
        var permissions = await _roleService.GetAllPermissionsAsync(cancellationToken);
        return Response(permissions);
    }
}
