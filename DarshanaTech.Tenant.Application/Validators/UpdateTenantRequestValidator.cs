using FluentValidation;
using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Validators;

public class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.SchoolName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.SchoolName));

        RuleFor(x => x.AdminEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.AdminEmail));

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Currency)
            .MaximumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.ExpiryDate)
            .Must(d => !d.HasValue || d.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("Expiry date cannot be in the past");
    }
}
