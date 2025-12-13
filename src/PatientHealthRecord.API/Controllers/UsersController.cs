using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Users;
using PatientHealthRecord.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace PatientHealthRecord.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all users with pagination")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _userService.GetAllAsync(page, pageSize, cancellationToken);
        return Response(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get user by ID")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return Response(user);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new user")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update user information")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return Response(user);
    }

    [HttpPut("{id}/activate")]
    [SwaggerOperation(Summary = "Activate user account")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var request = new UpdateUserRequest { IsActive = true };
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return Response(user);
    }

    [HttpPut("{id}/deactivate")]
    [SwaggerOperation(Summary = "Deactivate user account")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userService.DeactivateAsync(id, cancellationToken);
        return Response(user, "User account has been deactivated successfully");
    }

    [HttpPost("{userId}/roles/{roleId}")]
    [SwaggerOperation(Summary = "Assign role to user")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.AssignRoleAsync(userId, roleId, cancellationToken);
        return Response(result);
    }

    [HttpDelete("{userId}/roles/{roleId}")]
    [SwaggerOperation(Summary = "Remove role from user")]
    [Authorize(Policy = Permissions.ManageUsers)]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.RemoveRoleAsync(userId, roleId, cancellationToken);
        return Response(result);
    }
}
