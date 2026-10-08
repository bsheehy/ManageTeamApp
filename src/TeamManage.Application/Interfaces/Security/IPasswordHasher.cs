namespace TeamManage.Application.Interfaces.Security;

/// <summary>
/// Hashes and verifies user passwords. Implemented in Infrastructure using a
/// strong, salted algorithm (e.g. ASP.NET Core Identity's PasswordHasher or BCrypt).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
