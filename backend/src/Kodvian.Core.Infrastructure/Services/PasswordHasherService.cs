using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kodvian.Core.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new User(), password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(new User(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
