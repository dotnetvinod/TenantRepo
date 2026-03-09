namespace DarshanaTech.Tenant.Application.DTOs;

public record AssignPlanRequest
{
    public int PlanId { get; init; }
    public DateTime? ExpiryDate { get; init; }
}
