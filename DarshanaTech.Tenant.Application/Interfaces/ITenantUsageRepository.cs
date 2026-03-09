using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface ITenantUsageRepository
{
    Task<TenantUsage?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantUsage> AddAsync(TenantUsage usage, CancellationToken cancellationToken = default);
    void Update(TenantUsage usage);
}
