using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// A scheduled fixture for a team.
/// </summary>
public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public string Opponent { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime KickOff { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<MatchPlayer> Selections { get; set; } = new List<MatchPlayer>();
}
