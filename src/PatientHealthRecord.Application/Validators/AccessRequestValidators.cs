using FluentValidation;
using PatientHealthRecord.Application.DTOs.AccessRequests;

namespace PatientHealthRecord.Application.Validators;

public class CreateAccessRequestRequestValidator : AbstractValidator<CreateAccessRequestRequest>
{
    public CreateAccessRequestRequestValidator()
    {
        RuleFor(x => x.HealthRecordId)
            .NotEmpty().WithMessage("Health record ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}

public class ApproveAccessRequestRequestValidator : AbstractValidator<ApproveAccessRequestRequest>
{
    public ApproveAccessRequestRequestValidator()
    {
        RuleFor(x => x.AccessStartDateTime)
            .NotEmpty().WithMessage("Access start date/time is required")
            .Must(date => date.Date >= DateTime.UtcNow.Date)
            .WithMessage("Access start date/time must be today or in the future");

        RuleFor(x => x.AccessEndDateTime)
            .NotEmpty().WithMessage("Access end date/time is required")
            .GreaterThan(x => x.AccessStartDateTime)
            .WithMessage("Access end date/time must be after start date/time");

        RuleFor(x => x.ReviewComment)
            .MaximumLength(500).WithMessage("Review comment must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ReviewComment));
    }
}

public class DeclineAccessRequestRequestValidator : AbstractValidator<DeclineAccessRequestRequest>
{
    public DeclineAccessRequestRequestValidator()
    {
        RuleFor(x => x.ReviewComment)
            .NotEmpty().WithMessage("Review comment is required when declining")
            .MinimumLength(10).WithMessage("Review comment must be at least 10 characters")
            .MaximumLength(500).WithMessage("Review comment must not exceed 500 characters");
    }
}
