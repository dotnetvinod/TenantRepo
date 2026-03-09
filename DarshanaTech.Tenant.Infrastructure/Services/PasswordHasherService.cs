using Microsoft.AspNetCore.Identity;
using DarshanaTech.Tenant.Application.Interfaces;

namespace DarshanaTech.Tenant.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    private static readonly PasswordHasher<DummyUser> Hasher = new();

    public string HashPassword(string password) =>
        Hasher.HashPassword(null!, password);

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = Hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }
}
