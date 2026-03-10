using FluentValidation;
using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Validators;

public class AddPlanRequestValidator : AbstractValidator<AddPlanRequest>
{
    public AddPlanRequestValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(100).WithMessage("Plan name cannot exceed 100 characters");

        RuleFor(x => x.MaxStudents)
            .GreaterThanOrEqualTo(0).WithMessage("Max students cannot be negative");

        RuleFor(x => x.MaxTeachers)
            .GreaterThanOrEqualTo(0).WithMessage("Max teachers cannot be negative");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.FeaturesJson)
            .MaximumLength(4000).WithMessage("Features JSON cannot exceed 4000 characters")
            .Must(BeValidJson).WithMessage("FeaturesJson must be valid JSON");
    }

    private static bool BeValidJson(string? value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        try
        {
            System.Text.Json.JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
