using FluentValidation;
using PatientHealthRecord.Application.DTOs.HealthRecords;

namespace PatientHealthRecord.Application.Validators;

public class CreateHealthRecordRequestValidator : AbstractValidator<CreateHealthRecordRequest>
{
    public CreateHealthRecordRequestValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required")
            .MaximumLength(1000).WithMessage("Diagnosis must not exceed 1000 characters");

        RuleFor(x => x.TreatmentPlan)
            .NotEmpty().WithMessage("Treatment plan is required")
            .MaximumLength(2000).WithMessage("Treatment plan must not exceed 2000 characters");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(5000).WithMessage("Medical history must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.MedicalHistory));
    }
}

public class UpdateHealthRecordRequestValidator : AbstractValidator<UpdateHealthRecordRequest>
{
    public UpdateHealthRecordRequestValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required")
            .MaximumLength(1000).WithMessage("Diagnosis must not exceed 1000 characters");

        RuleFor(x => x.TreatmentPlan)
            .NotEmpty().WithMessage("Treatment plan is required")
            .MaximumLength(2000).WithMessage("Treatment plan must not exceed 2000 characters");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(5000).WithMessage("Medical history must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.MedicalHistory));
    }
}
