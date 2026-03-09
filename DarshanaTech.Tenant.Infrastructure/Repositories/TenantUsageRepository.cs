using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using DarshanaTech.Tenant.Infrastructure.Data;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class TenantUsageRepository : ITenantUsageRepository
{
    private readonly ApplicationDbContext _context;

    public TenantUsageRepository(ApplicationDbContext context) => _context = context;

    public async Task<TenantUsage?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.TenantUsage
            .FirstOrDefaultAsync(u => u.TenantId == tenantId, cancellationToken);

    public async Task<TenantUsage> AddAsync(TenantUsage usage, CancellationToken cancellationToken = default)
    {
        await _context.TenantUsage.AddAsync(usage, cancellationToken);
        return usage;
    }

    public void Update(TenantUsage usage) => _context.TenantUsage.Update(usage);
}
