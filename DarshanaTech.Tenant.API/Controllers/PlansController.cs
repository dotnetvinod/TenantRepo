using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarshanaTech.Tenant.Application.Interfaces;

namespace DarshanaTech.Tenant.API.Controllers;

/// <summary>
/// API for subscription plans. SuperAdmin-only. Used by UI for plan dropdowns and assign-plan.
/// </summary>
[ApiController]
[Route("api/superadmin/plans")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin")]
public class PlansController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PlansController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>Get all subscription plans</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var plans = await _unitOfWork.Plans.GetAllAsync(cancellationToken);
        var result = plans.Select(p => new { p.PlanId, p.PlanName, p.MaxStudents, p.MaxTeachers, p.Price });
        return Ok(result);
    }
}
