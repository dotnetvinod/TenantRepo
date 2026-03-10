using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Plan> AddAsync(Plan plan, CancellationToken cancellationToken = default);
}
