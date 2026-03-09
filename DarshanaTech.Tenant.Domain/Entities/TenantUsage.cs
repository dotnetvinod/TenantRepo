namespace DarshanaTech.Tenant.Domain.Entities;

public class TenantUsage
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public long StorageUsed { get; set; } // in bytes
    public DateTime LastUpdated { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
