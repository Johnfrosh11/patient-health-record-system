namespace PatientHealthRecord.Utilities;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public string Key        { get; set; } = string.Empty;
    public string Issuer     { get; set; } = string.Empty;
    public string Audience   { get; set; } = string.Empty;
    public int    JwtExpires { get; set; } = 28800; // 8 hours default
}
