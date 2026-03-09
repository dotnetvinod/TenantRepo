using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface ITenantCacheService
{
    Task<TenantResponse?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task SetAsync(string subdomain, TenantResponse tenant, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string subdomain, CancellationToken cancellationToken = default);
    Task InvalidateByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
