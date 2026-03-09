namespace DarshanaTech.Tenant.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Domain.Enums.UserRole Role { get; set; }
    public Domain.Enums.UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
