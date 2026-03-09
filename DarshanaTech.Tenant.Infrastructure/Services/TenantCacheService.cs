using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DarshanaTech.Tenant.Application.DTOs;
using DarshanaTech.Tenant.Application.Interfaces;
using System.Text.Json;

namespace DarshanaTech.Tenant.Infrastructure.Services;

public class TenantCacheService : ITenantCacheService
{
    private const string KeyPrefix = "tenant:subdomain:";
    private const int SlidingExpirationMinutes = 15;
    private readonly IDistributedCache _cache;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantCacheService> _logger;

    public TenantCacheService(IDistributedCache cache, ITenantRepository tenantRepository, ILogger<TenantCacheService> logger)
    {
        _cache = cache;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<TenantResponse?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + subdomain.ToLowerInvariant();
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            try
            {
                return JsonSerializer.Deserialize<TenantResponse>(cached);
            }
            catch (JsonException)
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
        }

        var tenant = await _tenantRepository.GetBySubdomainAsync(subdomain, cancellationToken);
        if (tenant != null)
        {
            var response = MapToResponse(tenant);
            await SetAsync(subdomain, response, cancellationToken);
            return response;
        }

        return null;
    }

    private static TenantResponse MapToResponse(DarshanaTech.Tenant.Domain.Entities.Tenant tenant) => new()
    {
        TenantId = tenant.TenantId,
        SchoolName = tenant.SchoolName,
        SchoolAddress = tenant.SchoolAddress,
        Subdomain = tenant.Subdomain,
        AdminEmail = tenant.AdminEmail,
        PlanId = tenant.PlanId,
        PlanName = tenant.Plan?.PlanName ?? string.Empty,
        Status = tenant.Status.ToString(),
        CreatedAt = tenant.CreatedAt,
        ExpiryDate = tenant.ExpiryDate,
        CustomDomain = tenant.CustomDomain,
        BrandLogo = tenant.BrandLogo,
        Timezone = tenant.Timezone,
        Country = tenant.Country,
        Currency = tenant.Currency,
        TenantSettingsJson = tenant.TenantSettingsJson,
        FeatureFlagsJson = tenant.FeatureFlagsJson
    };

    public async Task SetAsync(string subdomain, TenantResponse tenant, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + subdomain.ToLowerInvariant();
        var json = JsonSerializer.Serialize(tenant);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(SlidingExpirationMinutes)
        }, cancellationToken);
    }

    public async Task InvalidateAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + subdomain.ToLowerInvariant();
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogDebug("Invalidated tenant cache for subdomain: {Subdomain}", subdomain);
    }

    public async Task InvalidateByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant != null)
            await InvalidateAsync(tenant.Subdomain, cancellationToken);
    }
}
