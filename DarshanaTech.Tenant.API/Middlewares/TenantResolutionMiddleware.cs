using System.Text.RegularExpressions;

namespace DarshanaTech.Tenant.API.Middlewares;

/// <summary>
/// Extracts tenant subdomain from host (e.g. sunrise.yoursaas.com -> sunrise) and stores in HttpContext.Items["TenantSubdomain"].
/// Skips localhost and IP addresses. Used by TenantSuspensionMiddleware for subdomain-based blocking.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Parses host, extracts subdomain, stores in context.Items, then invokes next.</summary>
    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var host = context.Request.Host.Host ?? string.Empty;

        // Extract subdomain: sunrise.yoursaas.com -> sunrise
        var subdomain = ExtractSubdomain(host);

        if (!string.IsNullOrEmpty(subdomain))
        {
            context.Items["TenantSubdomain"] = subdomain.ToLowerInvariant();
            _logger.LogDebug("Resolved tenant subdomain: {Subdomain} from host: {Host}", subdomain, host);
        }

        await _next(context);
    }

    /// <summary>Returns first part of host before first dot, or null for localhost/IP.</summary>
    private static string? ExtractSubdomain(string host)
    {
        // Skip localhost and IP addresses
        if (host is "localhost" or "127.0.0.1" || Regex.IsMatch(host, @"^\d+\.\d+\.\d+\.\d+$"))
            return null;

        var parts = host.Split('.');
        if (parts.Length >= 2)
            return parts[0];

        return null;
    }
}
