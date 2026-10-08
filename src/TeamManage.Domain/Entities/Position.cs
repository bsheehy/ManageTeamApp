namespace TeamManage.Domain.Entities;

/// <summary>
/// A playing position belonging to a team's formation (e.g. Goalkeeper, Full Back,
/// Centre Forward). Used when allocating players to a starting lineup.
/// </summary>
public class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order of the position within the formation (1 = first position, etc.).
    /// </summary>
    public int SortOrder { get; set; }

    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
}
