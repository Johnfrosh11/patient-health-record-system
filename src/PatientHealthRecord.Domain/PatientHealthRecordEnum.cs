namespace PatientHealthRecord.Domain;

/// <summary>
/// All enums for the entire solution in a single file
/// </summary>

public enum AccessRequestStatus
{
    Pending   = 0,    // default — always 0
    Approved  = 1,
    Rejected  = 2,
    Expired   = 3,
}

public enum AuditAction
{
    Unknown        = 0,
    Create         = 1,
    Read           = 2,
    Update         = 3,
    Delete         = 4,
    Login          = 5,
    Logout         = 6,
    AccessGranted  = 7,
    AccessDenied   = 8,
    Export         = 9,
}

public enum PermissionType
{
    Unknown           = 0,
    CreateRecord      = 1,
    ReadRecord        = 2,
    UpdateRecord      = 3,
    DeleteRecord      = 4,
    ManageUsers       = 5,
    ManageRoles       = 6,
    ManagePermissions = 7,
    ApproveAccess     = 8,
    ViewAuditLogs     = 9,
}
