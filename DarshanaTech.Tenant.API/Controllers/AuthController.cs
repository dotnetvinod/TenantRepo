using DarshanaTech.Tenant.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarshanaTech.Tenant.API.Controllers;

/// <summary>
/// Proxies authentication requests to the shared Auth Web API.
/// UI calls this; Tenant API forwards to DarshanaTech.AuthWebAPI and returns the JWT.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthApiClient _authApiClient;

    public AuthController(IAuthApiClient authApiClient) => _authApiClient = authApiClient;

    /// <summary>Proxies login to Auth Web API. Returns JWT token on success.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var (success, token, expiresIn, connectionError) = await _authApiClient.LoginAsync(
            request.Email ?? "", request.Password ?? "", cancellationToken);

        if (success && !string.IsNullOrEmpty(token))
            return Ok(new { token, expiresIn = expiresIn ?? 3600 });

        if (connectionError)
            return StatusCode(503, new { error = "Authentication service is temporarily unavailable. Please try again later." });

        return Unauthorized(new { error = "Invalid credentials" });
    }
}

public record LoginRequest(string? Email, string? Password);
