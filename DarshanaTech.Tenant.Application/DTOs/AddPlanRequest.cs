namespace DarshanaTech.Tenant.Application.DTOs;

public record AddPlanRequest
{
    public string PlanName { get; init; } = string.Empty;
    public int MaxStudents { get; init; }
    public int MaxTeachers { get; init; }
    public decimal Price { get; init; }
    public string FeaturesJson { get; init; } = "[]";
}
