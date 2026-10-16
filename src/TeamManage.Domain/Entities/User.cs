using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// An application user. Can be a Club Manager (owns clubs), a Team Manager (runs teams assigned
/// by a club manager) or a Player (belongs to teams via <see cref="TeamPlayer"/>). A user can
/// manage one team and play on another.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Salted password hash. Never store plain-text passwords.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    /// <summary>
    /// Hash of the one-time invite (magic link) token, cleared once redeemed.
    /// </summary>
    public string? InviteTokenHash { get; set; }

    public DateTime? InviteTokenExpiresAtUtc { get; set; }

    /// <summary>
    /// True for invited accounts that still have a system-generated password and must choose their own.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// The club manager who created this team manager account (null for self-registered users).
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Club> ManagedClubs { get; set; } = new List<Club>();

    public ICollection<Team> ManagedTeams { get; set; } = new List<Team>();

    public ICollection<TeamPlayer> TeamMemberships { get; set; } = new List<TeamPlayer>();

    public ICollection<MatchPlayer> MatchSelections { get; set; } = new List<MatchPlayer>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
