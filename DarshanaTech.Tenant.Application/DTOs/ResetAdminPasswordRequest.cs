namespace DarshanaTech.Tenant.Application.DTOs;

public record ResetAdminPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
