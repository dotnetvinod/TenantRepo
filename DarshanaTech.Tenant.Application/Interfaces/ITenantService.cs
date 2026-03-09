using DarshanaTech.Tenant.Application.DTOs;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface ITenantService
{
    Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request, string performedBy, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantResponse>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantResponse>> GetFilteredTenantsAsync(string? status, string? search, int? planId, CancellationToken cancellationToken = default);
    Task<PagedResult<TenantResponse>> GetFilteredTenantsPagedAsync(string? status, string? search, int? planId, int page, int pageSize, string? sortBy, bool sortAsc, CancellationToken cancellationToken = default);
    Task<TenantResponse?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantResponse?> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<TenantResponse?> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, string performedBy, CancellationToken cancellationToken = default);
    Task<bool> SuspendTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default);
    Task<bool> ActivateTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default);
    Task<bool> ResetAdminPasswordAsync(Guid tenantId, ResetAdminPasswordRequest request, string performedBy, CancellationToken cancellationToken = default);
    Task<TenantUsageResponse?> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> AssignPlanAsync(Guid tenantId, AssignPlanRequest request, string performedBy, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantResponse>> GetExpiringTenantsAsync(int withinDays = 30, CancellationToken cancellationToken = default);
}
