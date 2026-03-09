namespace PatientHealthRecord.Utilities;

/// <summary>
/// Base class for paginated search requests
/// </summary>
public class BaseSearchDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize   { get; set; } = 20;
    public string? Search { get; set; }
}
