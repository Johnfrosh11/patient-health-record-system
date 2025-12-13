namespace PatientHealthRecord.Application.Common.Models;

public static class ErrorCodes
{
    public const string Successful = "SUCCESS";
    public const string Failed = "FAILED";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadRequest = "BAD_REQUEST";
}
