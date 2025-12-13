using FluentValidation;
using PatientHealthRecord.Application.DTOs.Roles;

namespace PatientHealthRecord.Application.Validators;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3).WithMessage("Role name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Role name must not exceed 50 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters");
        });
    }
}
