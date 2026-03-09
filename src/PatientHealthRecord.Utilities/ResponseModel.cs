namespace PatientHealthRecord.Utilities;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
public class ResponseModel<T>
{
    public string  code    { get; set; } = string.Empty;
    public bool    success { get; set; }
    public string  message { get; set; } = string.Empty;
    public T?      data    { get; set; }
}
