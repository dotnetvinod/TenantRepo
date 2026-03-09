namespace DarshanaTech.Tenant.API.Services;

/// <summary>
/// Client for the shared Auth Web API. Tenant API uses this to obtain JWT tokens for login.
/// </summary>
public interface IAuthApiClient
{
    Task<(bool Success, string? Token, int? ExpiresIn, bool ConnectionError)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
