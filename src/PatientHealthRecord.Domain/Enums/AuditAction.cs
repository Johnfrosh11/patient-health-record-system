namespace PatientHealthRecord.Domain.Enums;

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    View = 4,
    Login = 5,
    Logout = 6,
    ApproveAccess = 7,
    DeclineAccess = 8,
    GrantPermission = 9,
    RevokePermission = 10,
    Export = 11,
    AccessDenied = 12
}
