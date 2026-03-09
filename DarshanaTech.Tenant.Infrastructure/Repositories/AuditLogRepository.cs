using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using DarshanaTech.Tenant.Infrastructure.Data;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) =>
        await _context.AuditLogs.AddAsync(log, cancellationToken);
}
