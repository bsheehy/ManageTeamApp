using TeamManage.Domain.Enums;

namespace TeamManage.Domain.Entities;

/// <summary>
/// Represents a single player's invitation to, attendance response for, and
/// (optional) lineup allocation within a <see cref="Match"/>.
/// A row is created for every player in the team's panel when a match is scheduled.
/// </summary>
public class MatchPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MatchId { get; set; }

    public Match? Match { get; set; }

    public Guid PlayerId { get; set; }

    public User? Player { get; set; }

    /// <summary>
    /// The position allocated to this player for the match. Null if not yet
    /// allocated, or if the player is not part of the selected squad.
    /// </summary>
    public Guid? PositionId { get; set; }

    public Position? Position { get; set; }

    /// <summary>
    /// True if allocated to the starting lineup; false if a substitute.
    /// Only meaningful when <see cref="IsSelected"/> is true.
    /// </summary>
    public bool IsStarter { get; set; }

    /// <summary>
    /// True once the manager has included this player in the match-day squad
    /// (starters + substitutes).
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// The player's own RSVP response to the match invitation.
    /// </summary>
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Pending;

    public DateTime? RespondedAt { get; set; }
}
