namespace PatientHealthRecord.Utilities;

/// <summary>
/// Global application settings from appsettings.json
/// </summary>
public class GlobalSettings
{
    public EmailSettings       EmailSettings       { get; set; } = new();
    public SmsSettings         SmsSettings         { get; set; } = new();
    public AzureBlobSettings   AzureBlobSettings   { get; set; } = new();
    public bool                MigrationMode       { get; set; }
}

public class EmailSettings
{
    public string ApiKey    { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName  { get; set; } = string.Empty;
}

public class SmsSettings
{
    public string ApiKey  { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

public class AzureBlobSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Container        { get; set; } = string.Empty;
    public string BaseUrl          { get; set; } = string.Empty;
}
