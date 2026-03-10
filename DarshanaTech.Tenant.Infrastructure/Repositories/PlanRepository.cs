using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using DarshanaTech.Tenant.Infrastructure.Data;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly ApplicationDbContext _context;

    public PlanRepository(ApplicationDbContext context) => _context = context;

    public async Task<Plan?> GetByIdAsync(int planId, CancellationToken cancellationToken = default) =>
        await _context.Plans.FindAsync([planId], cancellationToken);

    public async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Plans.OrderBy(p => p.PlanId).ToListAsync(cancellationToken);

    public async Task<Plan> AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        await _context.Plans.AddAsync(plan, cancellationToken);
        return plan;
    }
}
