using FluentValidation;
using PatientHealthRecord.Application.DTOs.Roles;

namespace PatientHealthRecord.Application.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(3).WithMessage("Role name must be at least 3 characters")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionIds)
            .NotEmpty().WithMessage("At least one permission must be assigned");
    }
}
