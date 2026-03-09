using Microsoft.Extensions.Logging;
using DarshanaTech.Tenant.Application.DTOs;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using DarshanaTech.Tenant.Domain.Enums;
using TenantEntity = DarshanaTech.Tenant.Domain.Entities.Tenant;

namespace DarshanaTech.Tenant.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantCacheService? _tenantCache;
    private readonly ILogger<TenantService> _logger;

    public TenantService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<TenantService> logger, ITenantCacheService? tenantCache)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tenantCache = tenantCache;
        _logger = logger;
    }

    public async Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request, string performedBy, CancellationToken cancellationToken = default)
    {
        var subdomain = request.Subdomain.ToLowerInvariant();
        if (await _unitOfWork.Tenants.SubdomainExistsAsync(subdomain, null, cancellationToken))
            throw new InvalidOperationException($"Subdomain '{subdomain}' is already in use.");

        var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new InvalidOperationException($"Plan with ID {request.PlanId} does not exist.");

        var tenantId = Guid.NewGuid();
        var tenant = new TenantEntity
        {
            TenantId = tenantId,
            SchoolName = request.SchoolName,
            SchoolAddress = request.SchoolAddress,
            Subdomain = subdomain,
            AdminEmail = request.AdminEmail,
            PlanId = request.PlanId,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate,
            CustomDomain = request.CustomDomain,
            BrandLogo = request.BrandLogo,
            Timezone = request.Timezone,
            Country = request.Country,
            Currency = request.Currency,
            TenantSettingsJson = request.TenantSettingsJson ?? "{}",
            FeatureFlagsJson = request.FeatureFlagsJson ?? "{}"
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);

        var schoolAdmin = new User
        {
            UserId = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.AdminEmail,
            PasswordHash = _passwordHasher.HashPassword(request.AdminPassword),
            Role = UserRole.SchoolAdmin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(schoolAdmin, cancellationToken);

        var usage = new TenantUsage
        {
            TenantId = tenantId,
            TotalStudents = 0,
            TotalTeachers = 0,
            StorageUsed = 0,
            LastUpdated = DateTime.UtcNow
        };
        await _unitOfWork.TenantUsage.AddAsync(usage, cancellationToken);

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "TenantCreated",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { request.SchoolName, request.Subdomain })
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant {TenantId} created for school {SchoolName} by {PerformedBy}",
            tenantId, request.SchoolName, performedBy);

        return MapToResponse(tenant, plan.PlanName);
    }

    public async Task<IEnumerable<TenantResponse>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
        return tenants.Select(t => MapToResponse(t, t.Plan?.PlanName ?? string.Empty));
    }

    public async Task<IEnumerable<TenantResponse>> GetFilteredTenantsAsync(string? status, string? search, int? planId, CancellationToken cancellationToken = default)
    {
        var tenants = await _unitOfWork.Tenants.GetFilteredAsync(status, search, planId, cancellationToken);
        return tenants.Select(t => MapToResponse(t, t.Plan?.PlanName ?? string.Empty));
    }

    public async Task<PagedResult<TenantResponse>> GetFilteredTenantsPagedAsync(string? status, string? search, int? planId, int page, int pageSize, string? sortBy, bool sortAsc, CancellationToken cancellationToken = default)
    {
        await EnsureExpiredTenantsSuspendedAsync(cancellationToken);
        var (items, totalCount) = await _unitOfWork.Tenants.GetFilteredPagedAsync(status, search, planId, page, pageSize, sortBy, sortAsc, cancellationToken);
        var responses = items.Select(t => MapToResponse(t, t.Plan?.PlanName ?? string.Empty));
        return new PagedResult<TenantResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<TenantResponse?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await EnsureExpiredTenantsSuspendedAsync(cancellationToken);
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        return tenant == null ? null : MapToResponse(tenant, tenant.Plan?.PlanName ?? string.Empty);
    }

    public async Task<TenantResponse?> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySubdomainAsync(subdomain.ToLowerInvariant(), cancellationToken);
        return tenant == null ? null : MapToResponse(tenant, tenant.Plan?.PlanName ?? string.Empty);
    }

    public async Task<TenantResponse?> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return null;

        if (!string.IsNullOrEmpty(request.SchoolName)) tenant.SchoolName = request.SchoolName;
        if (request.SchoolAddress != null) tenant.SchoolAddress = request.SchoolAddress;
        if (!string.IsNullOrEmpty(request.AdminEmail)) tenant.AdminEmail = request.AdminEmail;
        if (request.ExpiryDate.HasValue) tenant.ExpiryDate = request.ExpiryDate;
        if (request.CustomDomain != null) tenant.CustomDomain = request.CustomDomain;
        if (request.BrandLogo != null) tenant.BrandLogo = request.BrandLogo;
        if (!string.IsNullOrEmpty(request.Timezone)) tenant.Timezone = request.Timezone;
        if (!string.IsNullOrEmpty(request.Country)) tenant.Country = request.Country;
        if (!string.IsNullOrEmpty(request.Currency)) tenant.Currency = request.Currency;
        if (request.TenantSettingsJson != null) tenant.TenantSettingsJson = request.TenantSettingsJson;
        if (request.FeatureFlagsJson != null) tenant.FeatureFlagsJson = request.FeatureFlagsJson;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "TenantUpdated",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow,
            Metadata = System.Text.Json.JsonSerializer.Serialize(request)
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_tenantCache != null) await _tenantCache.InvalidateByTenantIdAsync(tenantId, cancellationToken);
        _logger.LogInformation("Tenant {TenantId} updated by {PerformedBy}", tenantId, performedBy);
        return MapToResponse(tenant, tenant.Plan?.PlanName ?? string.Empty);
    }

    public async Task<bool> SuspendTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        tenant.Status = TenantStatus.Suspended;
        _unitOfWork.Tenants.Update(tenant);

        var users = await _unitOfWork.Users.GetByTenantIdAsync(tenantId, cancellationToken);
        foreach (var user in users)
        {
            user.Status = UserStatus.Suspended;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "TenantSuspended",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (_tenantCache != null) await _tenantCache.InvalidateByTenantIdAsync(tenantId, cancellationToken);

        _logger.LogWarning("Tenant {TenantId} suspended by {PerformedBy}", tenantId, performedBy);
        return true;
    }

    public async Task<bool> ActivateTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        tenant.Status = TenantStatus.Active;
        _unitOfWork.Tenants.Update(tenant);

        var users = await _unitOfWork.Users.GetByTenantIdAsync(tenantId, cancellationToken);
        foreach (var user in users)
        {
            if (user.Status == UserStatus.Suspended)
            {
                user.Status = UserStatus.Active;
                _unitOfWork.Users.Update(user);
            }
        }

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "TenantActivated",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (_tenantCache != null) await _tenantCache.InvalidateByTenantIdAsync(tenantId, cancellationToken);

        _logger.LogInformation("Tenant {TenantId} activated by {PerformedBy}", tenantId, performedBy);
        return true;
    }

    public async Task<bool> SoftDeleteTenantAsync(Guid tenantId, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        tenant.Status = TenantStatus.Deleted;
        _unitOfWork.Tenants.Update(tenant);

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "TenantDeleted",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (_tenantCache != null) await _tenantCache.InvalidateByTenantIdAsync(tenantId, cancellationToken);

        _logger.LogWarning("Tenant {TenantId} soft deleted by {PerformedBy}", tenantId, performedBy);
        return true;
    }

    public async Task<bool> ResetAdminPasswordAsync(Guid tenantId, ResetAdminPasswordRequest request, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        var adminUser = await _unitOfWork.Users.GetSchoolAdminByTenantAsync(tenantId, cancellationToken);
        if (adminUser == null) return false;

        await _unitOfWork.Users.ResetPasswordAsync(adminUser.UserId, request.NewPassword, cancellationToken);
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "AdminPasswordReset",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin password reset for tenant {TenantId} by {PerformedBy}", tenantId, performedBy);
        return true;
    }

    public async Task<TenantUsageResponse?> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return null;

        var usage = await _unitOfWork.TenantUsage.GetByTenantIdAsync(tenantId, cancellationToken);
        if (usage == null) return null;

        var plan = tenant.Plan;
        var maxStudents = plan?.MaxStudents ?? 0;
        var maxTeachers = plan?.MaxTeachers ?? 0;

        return new TenantUsageResponse
        {
            TenantId = tenantId,
            TotalStudents = usage.TotalStudents,
            TotalTeachers = usage.TotalTeachers,
            StorageUsed = usage.StorageUsed,
            LastUpdated = usage.LastUpdated,
            MaxStudents = maxStudents,
            MaxTeachers = maxTeachers,
            IsWithinLimits = usage.TotalStudents <= maxStudents && usage.TotalTeachers <= maxTeachers
        };
    }

    public async Task<bool> AssignPlanAsync(Guid tenantId, AssignPlanRequest request, string performedBy, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        var plan = await _unitOfWork.Plans.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null) return false;

        var usage = await _unitOfWork.TenantUsage.GetByTenantIdAsync(tenantId, cancellationToken);
        if (usage != null && (usage.TotalStudents > plan.MaxStudents || usage.TotalTeachers > plan.MaxTeachers))
            throw new InvalidOperationException("Current usage exceeds the new plan limits.");

        var oldPlanId = tenant.PlanId;
        tenant.PlanId = request.PlanId;
        if (request.ExpiryDate.HasValue) tenant.ExpiryDate = request.ExpiryDate;
        _unitOfWork.Tenants.Update(tenant);

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            Action = "PlanChanged",
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { OldPlanId = oldPlanId, NewPlanId = request.PlanId })
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (_tenantCache != null) await _tenantCache.InvalidateByTenantIdAsync(tenantId, cancellationToken);

        _logger.LogInformation("Plan changed for tenant {TenantId} to plan {PlanId} by {PerformedBy}", tenantId, request.PlanId, performedBy);
        return true;
    }

    public async Task<IEnumerable<TenantResponse>> GetExpiringTenantsAsync(int withinDays = 30, CancellationToken cancellationToken = default)
    {
        await EnsureExpiredTenantsSuspendedAsync(cancellationToken);
        var tenants = await _unitOfWork.Tenants.GetExpiringWithinDaysAsync(withinDays, cancellationToken);
        return tenants.Select(t => MapToResponse(t, t.Plan?.PlanName ?? "Unknown"));
    }

    private async Task EnsureExpiredTenantsSuspendedAsync(CancellationToken cancellationToken)
    {
        var suspendedIds = await _unitOfWork.Tenants.SuspendExpiredTenantsAsync(cancellationToken);
        if (suspendedIds.Count > 0)
        {
            foreach (var tenantId in suspendedIds)
            {
                var users = await _unitOfWork.Users.GetByTenantIdAsync(tenantId, cancellationToken);
                foreach (var user in users)
                {
                    user.Status = UserStatus.Suspended;
                    _unitOfWork.Users.Update(user);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (_tenantCache != null)
            {
                foreach (var id in suspendedIds)
                    await _tenantCache.InvalidateByTenantIdAsync(id, cancellationToken);
            }
            _logger.LogInformation("Auto-suspended {Count} expired tenant(s): {Ids}", suspendedIds.Count, string.Join(", ", suspendedIds));
        }
    }

    private static TenantResponse MapToResponse(TenantEntity tenant, string planName) => new()
    {
        TenantId = tenant.TenantId,
        SchoolName = tenant.SchoolName,
        SchoolAddress = tenant.SchoolAddress,
        Subdomain = tenant.Subdomain,
        AdminEmail = tenant.AdminEmail,
        PlanId = tenant.PlanId,
        PlanName = planName,
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
}
