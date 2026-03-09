namespace DarshanaTech.Tenant.Application.DTOs;

public record TenantUsageResponse
{
    public Guid TenantId { get; init; }
    public int TotalStudents { get; init; }
    public int TotalTeachers { get; init; }
    public long StorageUsed { get; init; }
    public DateTime LastUpdated { get; init; }
    public int MaxStudents { get; init; }
    public int MaxTeachers { get; init; }
    public bool IsWithinLimits { get; init; }
}
