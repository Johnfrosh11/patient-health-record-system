using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Models;
using System.Net;
using System.Security.Claims;

namespace PatientHealthRecord.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult NotifyModelStateError()
    {
        var errorMessages = new List<string>();
        var errors = ModelState.Values.SelectMany(v => v.Errors);
        
        foreach (var error in errors)
        {
            var msg = error.Exception == null ? error.ErrorMessage : error.Exception.Message;
            errorMessages.Add(msg);
        }

        ResponseModel response = new()
        {
            Code = ErrorCodes.ValidationError,
            Message = errorMessages.FirstOrDefault() ?? "Validation failed",
            Success = false
        };

        return BadRequest(response);
    }

    protected new IActionResult Response<T>(ApiResponse<T> result)
    {
        if (result?.StatusCode == (int)HttpStatusCode.OK)
        {
            return Ok(result);
        }

        return StatusCode(result?.StatusCode ?? 400, result);
    }

    protected new IActionResult Response(ResponseModel result)
    {
        if (result?.Code == ErrorCodes.Successful)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    protected new IActionResult Response<T>(T data, string message = "Success")
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        return Ok(response);
    }

    protected IActionResult ErrorResponse(string message, int statusCode = 400)
    {
        var response = ApiResponse<object>.ErrorResponse(message, statusCode);
        return StatusCode(statusCode, response);
    }

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
