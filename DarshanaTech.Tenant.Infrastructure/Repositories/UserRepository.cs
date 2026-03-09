using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using DarshanaTech.Tenant.Domain.Enums;
using DarshanaTech.Tenant.Infrastructure.Data;

namespace DarshanaTech.Tenant.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);

    public async Task<User?> GetSchoolAdminByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Role == UserRole.SchoolAdmin, cancellationToken);

    public async Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public void Update(User user) => _context.Users.Update(user);

    public async Task ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user != null)
        {
            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            _context.Users.Update(user);
        }
    }
}
