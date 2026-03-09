using FluentValidation;
using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Validators;

public class ResetAdminPasswordRequestValidator : AbstractValidator<ResetAdminPasswordRequest>
{
    public ResetAdminPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}
