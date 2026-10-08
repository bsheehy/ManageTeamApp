using Microsoft.AspNetCore.Identity;
using TeamManage.Application.Interfaces.Security;
using TeamManage.Domain.Entities;

namespace TeamManage.Infrastructure.Security;

/// <summary>
/// Hashes passwords using ASP.NET Core Identity's maintained <see cref="PasswordHasher{TUser}"/>
/// (PBKDF2-HMACSHA256, versioned format) instead of a hand-rolled implementation.
/// This lets hash parameters (e.g. iteration count) be upgraded later without a
/// separate migration - VerifyHashedPassword reports when a rehash is needed.
/// We don't use the rest of ASP.NET Core Identity (UserManager, stores, etc.) -
/// just this one maintained, widely-audited hashing primitive.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _identityHasher = new();

    public string Hash(string password) =>
        _identityHasher.HashPassword(user: null!, password);

    public bool Verify(string password, string hash)
    {
        var result = _identityHasher.VerifyHashedPassword(user: null!, hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
