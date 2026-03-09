namespace DarshanaTech.Tenant.Domain.Entities;

public class Tenant
{
    public Guid TenantId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolAddress { get; set; }
    public string Subdomain { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public Domain.Enums.TenantStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Advanced SaaS fields
    public string? CustomDomain { get; set; }
    public string? BrandLogo { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string TenantSettingsJson { get; set; } = "{}";
    public string FeatureFlagsJson { get; set; } = "{}";

    public Plan Plan { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
    public TenantUsage? Usage { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
