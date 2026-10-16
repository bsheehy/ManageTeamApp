using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// A named panel/team of players for a given sport, belonging to a club and run by a Team Manager.
/// </summary>
public class Team
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    /// <summary>
    /// Number of players that make up the starting lineup for this team's sport
    /// (e.g. 15 for Gaelic Football, 11 for Soccer, 5 for Basketball).
    /// Configurable so custom squad sizes/formats are supported.
    /// </summary>
    public int SquadSize { get; set; }

    public Guid ClubId { get; set; }

    public Club? Club { get; set; }

    /// <summary>
    /// The team manager assigned by the club manager. Null until one is assigned.
    /// </summary>
    public Guid? ManagerId { get; set; }

    public User? Manager { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// True when the user is this team's manager, or the manager of the club it belongs to.
    /// Requires <see cref="Club"/> to be loaded for the club manager check.
    /// </summary>
    public bool IsManagedBy(Guid userId) =>
        ManagerId == userId || Club?.ClubManagerId == userId;

    // Navigation properties
    public ICollection<TeamPlayer> Players { get; set; } = new List<TeamPlayer>();

    public ICollection<Position> Positions { get; set; } = new List<Position>();

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
