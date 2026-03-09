namespace DarshanaTech.Tenant.Application.DTOs;

public record CreateTenantRequest
{
    public string SchoolName { get; init; } = string.Empty;
    public string? SchoolAddress { get; init; }
    public string Subdomain { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public int PlanId { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CustomDomain { get; init; }
    public string? BrandLogo { get; init; }
    public string Timezone { get; init; } = "UTC";
    public string Country { get; init; } = string.Empty;
    public string Currency { get; init; } = "USD";
    public string? TenantSettingsJson { get; init; }
    public string? FeatureFlagsJson { get; init; }
}
