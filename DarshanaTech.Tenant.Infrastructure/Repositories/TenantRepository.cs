using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Infrastructure.Data;
using TenantEntity = DarshanaTech.Tenant.Domain.Entities.Tenant;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) => _context = context;

    public async Task<TenantEntity?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.Tenants
            .Include(t => t.Plan)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);

    public async Task<TenantEntity?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default) =>
        await _context.Tenants
            .Include(t => t.Plan)
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);

    public async Task<bool> SubdomainExistsAsync(string subdomain, Guid? excludeTenantId, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.Where(t => t.Subdomain == subdomain);
        if (excludeTenantId.HasValue)
            query = query.Where(t => t.TenantId != excludeTenantId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<TenantEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Tenants
            .Include(t => t.Plan)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<TenantEntity>> GetFilteredAsync(string? status, string? search, int? planId, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.Include(t => t.Plan).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DarshanaTech.Tenant.Domain.Enums.TenantStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (planId.HasValue && planId.Value > 0)
            query = query.Where(t => t.PlanId == planId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.SchoolName.ToLower().Contains(term) ||
                t.Subdomain.ToLower().Contains(term) ||
                (t.AdminEmail != null && t.AdminEmail.ToLower().Contains(term)));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TenantEntity> Items, int TotalCount)> GetFilteredPagedAsync(string? status, string? search, int? planId, int page, int pageSize, string? sortBy, bool sortAsc, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.Include(t => t.Plan).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DarshanaTech.Tenant.Domain.Enums.TenantStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (planId.HasValue && planId.Value > 0)
            query = query.Where(t => t.PlanId == planId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.SchoolName.ToLower().Contains(term) ||
                t.Subdomain.ToLower().Contains(term) ||
                (t.AdminEmail != null && t.AdminEmail.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "subdomain" => sortAsc ? query.OrderBy(t => t.Subdomain) : query.OrderByDescending(t => t.Subdomain),
            "adminemail" => sortAsc ? query.OrderBy(t => t.AdminEmail) : query.OrderByDescending(t => t.AdminEmail),
            "plan" => sortAsc ? query.OrderBy(t => t.Plan!.PlanName) : query.OrderByDescending(t => t.Plan!.PlanName),
            "expirydate" => sortAsc ? query.OrderBy(t => t.ExpiryDate) : query.OrderByDescending(t => t.ExpiryDate),
            _ => sortAsc ? query.OrderBy(t => t.SchoolName) : query.OrderByDescending(t => t.SchoolName)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Returns tenants (Active or Suspended) whose ExpiryDate is within the next N days or already passed.</summary>
    public async Task<IEnumerable<TenantEntity>> GetExpiringWithinDaysAsync(int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(withinDays);
        return await _context.Tenants
            .Include(t => t.Plan)
            .Where(t => t.ExpiryDate != null
                && t.ExpiryDate.Value.Date <= cutoff
                && t.Status != DarshanaTech.Tenant.Domain.Enums.TenantStatus.Deleted)
            .OrderBy(t => t.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Sets Status = Suspended for Active tenants whose ExpiryDate is before today. Returns tenant IDs suspended.</summary>
    public async Task<IReadOnlyList<Guid>> SuspendExpiredTenantsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var expired = await _context.Tenants
            .Where(t => t.ExpiryDate != null
                && t.ExpiryDate.Value.Date < today
                && t.Status == DarshanaTech.Tenant.Domain.Enums.TenantStatus.Active)
            .ToListAsync(cancellationToken);
        foreach (var t in expired)
            t.Status = DarshanaTech.Tenant.Domain.Enums.TenantStatus.Suspended;
        return expired.Select(t => t.TenantId).ToList();
    }

    public async Task<TenantEntity> AddAsync(TenantEntity tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        return tenant;
    }

    public void Update(TenantEntity tenant) => _context.Tenants.Update(tenant);
}
