using System.Text;
using System.Text.Json;

namespace DarshanaTech.Tenant.API.Services;

/// <summary>
/// HTTP client that calls the shared Auth Web API for JWT token generation.
/// </summary>
public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, int? ExpiresIn, bool ConnectionError)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(new { email, password });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("api/auth/login", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, null, null, false);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var token = root.TryGetProperty("token", out var t) ? t.GetString() : null;
            var expiresIn = root.TryGetProperty("expiresIn", out var e) ? e.GetInt32() : (int?)null;
            return (true, token, expiresIn, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Auth API for login");
            return (false, null, null, true);
        }
    }
}
