using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetSchoolAdminByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    Task ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);
}
