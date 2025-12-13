using PatientHealthRecord.Domain.Common;

namespace PatientHealthRecord.Domain.Entities;

public class HealthRecord : BaseEntity
{
    public string PatientName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }

    public User Creator { get; set; } = null!;
    public ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
}
