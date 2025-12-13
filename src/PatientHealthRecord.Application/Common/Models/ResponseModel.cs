namespace PatientHealthRecord.Application.Common.Models;

public class ResponseModel
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }

    public ResponseModel()
    {
    }

    public ResponseModel(string code, string message, bool success)
    {
        Code = code;
        Message = message;
        Success = success;
    }

    public static ResponseModel SuccessResponse(string message = "Operation completed successfully")
    {
        return new ResponseModel
        {
            Code = ErrorCodes.Successful,
            Message = message,
            Success = true
        };
    }

    public static ResponseModel ErrorResponse(string message, string? code = null)
    {
        return new ResponseModel
        {
            Code = code ?? ErrorCodes.Failed,
            Message = message,
            Success = false
        };
    }
}
