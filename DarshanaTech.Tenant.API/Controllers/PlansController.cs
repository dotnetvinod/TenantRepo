using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarshanaTech.Tenant.Application.DTOs;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Domain.Entities;
using FluentValidation;

namespace DarshanaTech.Tenant.API.Controllers;

/// <summary>
/// API for subscription plans. SuperAdmin-only. Used by UI for plan dropdowns, add plan, and assign-plan.
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

    /// <summary>Add a new subscription plan</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddPlanRequest request, [FromServices] IValidator<AddPlanRequest> validator, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var plan = new Plan
        {
            PlanName = request.PlanName,
            MaxStudents = request.MaxStudents,
            MaxTeachers = request.MaxTeachers,
            Price = request.Price,
            FeaturesJson = request.FeaturesJson
        };

        var added = await _unitOfWork.Plans.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { added.PlanId }, new { added.PlanId, added.PlanName, added.MaxStudents, added.MaxTeachers, added.Price });
    }
}
