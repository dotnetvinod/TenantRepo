using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Infrastructure.Data;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IPlanRepository? _plans;
    private ITenantUsageRepository? _tenantUsage;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context, _passwordHasher);
    public IPlanRepository Plans => _plans ??= new PlanRepository(_context);
    public ITenantUsageRepository TenantUsage => _tenantUsage ??= new TenantUsageRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
