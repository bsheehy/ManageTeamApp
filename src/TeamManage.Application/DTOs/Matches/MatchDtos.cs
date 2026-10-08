using TeamManage.Application.DTOs.Teams;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.DTOs.Matches;

public class MatchPlayerDto
{
    public Guid MatchPlayerId { get; set; }

    public Guid PlayerId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public Guid? PositionId { get; set; }

    public string? PositionName { get; set; }

    public bool IsStarter { get; set; }

    public bool IsSelected { get; set; }

    public AttendanceStatus AttendanceStatus { get; set; }

    public DateTime? RespondedAt { get; set; }
}

/// <summary>
/// Lightweight projection used for list views.
/// </summary>
public class MatchSummaryDto
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string Opponent { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime KickOff { get; set; }

    public MatchStatus Status { get; set; }

    /// <summary>
    /// The requesting player's own attendance status, populated only when the
    /// match is fetched in the context of a specific player.
    /// </summary>
    public AttendanceStatus? MyAttendanceStatus { get; set; }
}

/// <summary>
/// Full projection used for match detail views, including the full squad list.
/// </summary>
public class MatchDto
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string Opponent { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime KickOff { get; set; }

    public MatchStatus Status { get; set; }

    public string? Notes { get; set; }

    public List<MatchPlayerDto> Selections { get; set; } = new();

    /// <summary>
    /// The full set of positions defined for the team's formation, so a manager
    /// can allocate any player to any position regardless of current selection.
    /// </summary>
    public List<PositionDto> TeamPositions { get; set; } = new();
}

public class CreateMatchRequest
{
    public Guid TeamId { get; set; }

    public string Opponent { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime KickOff { get; set; }

    public string? Notes { get; set; }
}

public class AttendanceResponseRequest
{
    public AttendanceStatus Status { get; set; }
}

public class PlayerAllocationRequest
{
    public Guid MatchPlayerId { get; set; }

    public Guid? PositionId { get; set; }

    public bool IsStarter { get; set; }

    public bool IsSelected { get; set; }
}

public class AllocateSelectionsRequest
{
    public List<PlayerAllocationRequest> Allocations { get; set; } = new();
}
