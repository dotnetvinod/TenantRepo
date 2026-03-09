namespace DarshanaTech.Tenant.Application.DTOs;

public record UpdateTenantRequest
{
    public string? SchoolName { get; init; }
    public string? SchoolAddress { get; init; }
    public string? AdminEmail { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CustomDomain { get; init; }
    public string? BrandLogo { get; init; }
    public string? Timezone { get; init; }
    public string? Country { get; init; }
    public string? Currency { get; init; }
    public string? TenantSettingsJson { get; init; }
    public string? FeatureFlagsJson { get; init; }
}
