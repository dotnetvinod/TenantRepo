namespace DarshanaTech.Tenant.Application.Interfaces;

public interface ITenantRepository
{
    Task<DarshanaTech.Tenant.Domain.Entities.Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<DarshanaTech.Tenant.Domain.Entities.Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, Guid? excludeTenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<DarshanaTech.Tenant.Domain.Entities.Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DarshanaTech.Tenant.Domain.Entities.Tenant>> GetFilteredAsync(string? status, string? search, int? planId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<DarshanaTech.Tenant.Domain.Entities.Tenant> Items, int TotalCount)> GetFilteredPagedAsync(string? status, string? search, int? planId, int page, int pageSize, string? sortBy, bool sortAsc, CancellationToken cancellationToken = default);
    Task<IEnumerable<DarshanaTech.Tenant.Domain.Entities.Tenant>> GetExpiringWithinDaysAsync(int withinDays, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> SuspendExpiredTenantsAsync(CancellationToken cancellationToken = default);
    Task<DarshanaTech.Tenant.Domain.Entities.Tenant> AddAsync(DarshanaTech.Tenant.Domain.Entities.Tenant tenant, CancellationToken cancellationToken = default);
    void Update(DarshanaTech.Tenant.Domain.Entities.Tenant tenant);
}
