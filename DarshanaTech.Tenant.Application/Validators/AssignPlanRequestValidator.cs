using FluentValidation;
using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Validators;

public class AssignPlanRequestValidator : AbstractValidator<AssignPlanRequest>
{
    public AssignPlanRequestValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Subscription plan must exist");

        RuleFor(x => x.ExpiryDate)
            .Must(d => !d.HasValue || d.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("Expiry date cannot be in the past");
    }
}
