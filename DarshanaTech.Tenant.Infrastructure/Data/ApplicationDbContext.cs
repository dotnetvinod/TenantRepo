using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Domain.Entities;
using TenantEntity = DarshanaTech.Tenant.Domain.Entities.Tenant;

namespace DarshanaTech.Tenant.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantUsage> TenantUsage => Set<TenantUsage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
