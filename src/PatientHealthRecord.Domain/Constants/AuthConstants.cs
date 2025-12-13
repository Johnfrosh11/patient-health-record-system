namespace PatientHealthRecord.Domain.Constants;

public static class Permissions
{
    public const string ViewPatientRecords = "viewPatientRecords";
    public const string CreatePatientRecords = "createPatientRecords";
    public const string UpdatePatientRecords = "updatePatientRecords";
    public const string DeletePatientRecords = "deletePatientRecords";
    public const string ApproveAccessRequests = "approveAccessRequests";
    public const string ManageUsers = "manageUsers";
    public const string ManageRoles = "manageRoles";

    public static IEnumerable<string> GetAll()
    {
        return new[]
        {
            ViewPatientRecords,
            CreatePatientRecords,
            UpdatePatientRecords,
            DeletePatientRecords,
            ApproveAccessRequests,
            ManageUsers,
            ManageRoles
        };
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";

    public static IEnumerable<string> GetAll()
    {
        return new[] { Admin, Doctor, Nurse, Receptionist };
    }
}
