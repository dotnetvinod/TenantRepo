namespace DarshanaTech.Tenant.Domain.Entities;

public class AuditLog
{
    public long LogId { get; set; }
    public Guid? TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }
}
