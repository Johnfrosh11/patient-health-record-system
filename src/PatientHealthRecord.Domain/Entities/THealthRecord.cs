using PatientHealthRecord.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientHealthRecord.Domain.Entities;

/// <summary>
/// Health Record entity - contains patient medical records
/// </summary>
public class THealthRecord : BaseEntity
{
    [Key]
    public Guid     HealthRecordId   { get; set; } = Guid.NewGuid();
    public string   PatientName      { get; set; } = string.Empty;
    public DateTime DateOfBirth      { get; set; }
    public string   Diagnosis        { get; set; } = string.Empty;
    public string   TreatmentPlan    { get; set; } = string.Empty;
    public string?  MedicalHistory   { get; set; }
    public Guid     CreatedByUserId  { get; set; }

    // Navigation properties
    public TUser                       Creator        { get; set; } = null!;
    public ICollection<TAccessRequest> AccessRequests { get; set; } = [];
}
