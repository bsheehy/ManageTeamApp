namespace TeamManage.Domain.Entities;

/// <summary>
/// Join entity representing a player's membership of a team/panel.
/// A player may belong to multiple teams (e.g. club and county panels).
/// </summary>
public class TeamPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid PlayerId { get; set; }

    public User? Player { get; set; }

    public int? JerseyNumber { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
