using System.Net;

namespace PatientHealthRecord.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        StatusCode = (int)HttpStatusCode.OK;
        Message = message;
        Data = data;
    }

    public ApiResponse(int statusCode, string message, bool success = false)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = (int)HttpStatusCode.OK,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
