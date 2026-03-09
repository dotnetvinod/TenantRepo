using FluentValidation;
using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Validators;

public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("School name is required")
            .MaximumLength(200);

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain must contain only lowercase letters, numbers, and hyphens")
            .MaximumLength(63);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Admin password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Subscription plan must exist");

        RuleFor(x => x.ExpiryDate)
            .Must(d => !d.HasValue || d.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("Expiry date cannot be in the past");

        RuleFor(x => x.Country)
            .MaximumLength(100);

        RuleFor(x => x.Currency)
            .MaximumLength(3);
    }
}
