using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Utilities;
using System.Security.Claims;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Base controller for all API controllers
/// Single method: Response<T> — thin controller actions call this
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Single response mapper — the only place HTTP status mapping lives
    /// Controllers call: return Response(await service.DoStuff());
    /// </summary>
    protected IActionResult Response<T>(ResponseModel<T> result)
        => result?.code == "00" ? Ok(result) : BadRequest(result);

    /// <summary>
    /// Helper to get current user ID from JWT claims
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}
