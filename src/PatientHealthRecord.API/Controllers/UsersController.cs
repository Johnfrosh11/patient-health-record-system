using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.DTOs.Users;
using PatientHealthRecord.Application.Services.Users;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Users controller - user management operations (requires manageUsers permission)
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Policy = Permissions.ManageUsers)]
public sealed class UsersController(IUserService svc) : BaseController
{
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await svc.GetAllAsync(page, pageSize, ct));

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        => Ok(await svc.GetByIdAsync(id, ct));

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct = default)
        => Ok(await svc.CreateAsync(request, ct));

    /// <summary>
    /// Update user details
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct = default)
        => Ok(await svc.UpdateAsync(id, request, ct));

    /// <summary>
    /// Deactivate a user
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
        => Ok(await svc.DeactivateAsync(id, ct));

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> AssignRole(Guid userId, Guid roleId, CancellationToken ct = default)
        => Ok(await svc.AssignRoleAsync(userId, roleId, ct));

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken ct = default)
        => Ok(await svc.RemoveRoleAsync(userId, roleId, ct));
}
