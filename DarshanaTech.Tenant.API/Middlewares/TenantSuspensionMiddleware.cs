using DarshanaTech.Tenant.Application.Interfaces;

namespace DarshanaTech.Tenant.API.Middlewares;

/// <summary>
/// Blocks API access for suspended tenants when tenant is resolved from subdomain.
/// Super Admin endpoints use tenantId from route, so suspension is enforced in the service layer.
/// Uses Redis cache for tenant lookup when available.
/// </summary>
public class TenantSuspensionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantSuspensionMiddleware> _logger;

    public TenantSuspensionMiddleware(RequestDelegate next, ILogger<TenantSuspensionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>If TenantSubdomain present and tenant is Suspended, returns 403; otherwise continues pipeline.</summary>
    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var subdomain = context.Items["TenantSubdomain"] as string;
        if (string.IsNullOrEmpty(subdomain))
        {
            await _next(context);
            return;
        }

        // Skip for Super Admin routes - they manage tenants
        if (context.Request.Path.StartsWithSegments("/api/superadmin"))
        {
            await _next(context);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var tenantCache = scope.ServiceProvider.GetRequiredService<ITenantCacheService>();
        var tenant = await tenantCache.GetBySubdomainAsync(subdomain, context.RequestAborted);

        if (tenant != null && tenant.Status.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Blocked API access for suspended tenant: {Subdomain}", subdomain);
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Tenant account is suspended. Please contact support." });
            return;
        }

        await _next(context);
    }
}
