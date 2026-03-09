namespace DarshanaTech.Tenant.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IUserRepository Users { get; }
    IPlanRepository Plans { get; }
    ITenantUsageRepository TenantUsage { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
