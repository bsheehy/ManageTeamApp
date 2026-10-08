using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// A named panel/team of players for a given sport, owned by a Manager.
/// </summary>
public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    /// <summary>
    /// Number of players that make up the starting lineup for this team's sport
    /// (e.g. 15 for Gaelic Football, 11 for Soccer, 5 for Basketball).
    /// Configurable so custom squad sizes/formats are supported.
    /// </summary>
    public int SquadSize { get; set; }

    public Guid ManagerId { get; set; }

    public User? Manager { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<TeamPlayer> Players { get; set; } = new List<TeamPlayer>();

    public ICollection<Position> Positions { get; set; } = new List<Position>();

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
