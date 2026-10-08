using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// An application user. Can be a Manager (owns/administers teams) or a Player
/// (belongs to one or more teams via <see cref="TeamPlayer"/>).
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Salted password hash. Never store plain-text passwords.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Team> ManagedTeams { get; set; } = new List<Team>();

    public ICollection<TeamPlayer> TeamMemberships { get; set; } = new List<TeamPlayer>();

    public ICollection<MatchPlayer> MatchSelections { get; set; } = new List<MatchPlayer>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
