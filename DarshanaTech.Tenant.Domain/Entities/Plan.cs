namespace DarshanaTech.Tenant.Domain.Entities;

public class Plan
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int MaxTeachers { get; set; }
    public decimal Price { get; set; }
    public string FeaturesJson { get; set; } = "[]";

    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
