using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
}
