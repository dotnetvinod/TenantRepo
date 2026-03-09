using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarshanaTech.Tenant.Application.DTOs;
using DarshanaTech.Tenant.Application.Interfaces;

namespace DarshanaTech.Tenant.API.Controllers;

/// <summary>
/// SuperAdmin-only API for tenant CRUD, suspend/activate, delete, reset password, assign plan, and usage stats.
/// All endpoints require JWT Bearer with role SuperAdmin.
/// </summary>
[ApiController]
[Route("api/superadmin/tenants")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<SuperAdminController> _logger;

    public SuperAdminController(ITenantService tenantService, ILogger<SuperAdminController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>Create a new tenant (school)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request, [FromServices] IValidator<CreateTenantRequest> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        try
        {
            var performedBy = User.Identity?.Name ?? "Unknown";
            var result = await _tenantService.CreateTenantAsync(request, performedBy, cancellationToken);
            return CreatedAtAction(nameof(GetTenant), new { tenantId = result.TenantId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get tenants expiring within N days (for notifications). Excludes Deleted.</summary>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(IEnumerable<TenantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringTenants([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        if (days < 1) days = 30;
        if (days > 365) days = 365;
        var tenants = await _tenantService.GetExpiringTenantsAsync(days, cancellationToken);
        return Ok(tenants);
    }

    /// <summary>List all tenants with optional filters, paging, and sorting</summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTenants([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int? planId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] bool sortAsc = true, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 500) pageSize = 500;
        var result = await _tenantService.GetFilteredTenantsPagedAsync(status, search, planId, page, pageSize, sortBy, sortAsc, cancellationToken);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages });
    }

    /// <summary>Get tenant details by ID</summary>
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken);
        return tenant == null ? NotFound() : Ok(tenant);
    }

    /// <summary>Update tenant details</summary>
    [HttpPut("{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTenant(Guid tenantId, [FromBody] UpdateTenantRequest request, [FromServices] IValidator<UpdateTenantRequest> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var tenant = await _tenantService.UpdateTenantAsync(tenantId, request, User.Identity?.Name ?? "Unknown", cancellationToken);
        return tenant == null ? NotFound() : Ok(tenant);
    }

    /// <summary>Suspend tenant - blocks all user logins and API access</summary>
    [HttpPut("{tenantId:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var success = await _tenantService.SuspendTenantAsync(tenantId, User.Identity?.Name ?? "Unknown", cancellationToken);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Activate a suspended tenant</summary>
    [HttpPut("{tenantId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var success = await _tenantService.ActivateTenantAsync(tenantId, User.Identity?.Name ?? "Unknown", cancellationToken);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Soft delete tenant</summary>
    [HttpDelete("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var success = await _tenantService.SoftDeleteTenantAsync(tenantId, User.Identity?.Name ?? "Unknown", cancellationToken);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Reset school admin password</summary>
    [HttpPost("{tenantId:guid}/reset-admin-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetAdminPassword(Guid tenantId, [FromBody] ResetAdminPasswordRequest request, [FromServices] IValidator<ResetAdminPasswordRequest> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var success = await _tenantService.ResetAdminPasswordAsync(tenantId, request, User.Identity?.Name ?? "Unknown", cancellationToken);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Get tenant usage statistics</summary>
    [HttpGet("{tenantId:guid}/usage")]
    [ProducesResponseType(typeof(TenantUsageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantUsage(Guid tenantId, CancellationToken cancellationToken)
    {
        var usage = await _tenantService.GetTenantUsageAsync(tenantId, cancellationToken);
        return usage == null ? NotFound() : Ok(usage);
    }

    /// <summary>Assign or change subscription plan</summary>
    [HttpPut("{tenantId:guid}/assign-plan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignPlan(Guid tenantId, [FromBody] AssignPlanRequest request, [FromServices] IValidator<AssignPlanRequest> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        try
        {
            var success = await _tenantService.AssignPlanAsync(tenantId, request, User.Identity?.Name ?? "Unknown", cancellationToken);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
