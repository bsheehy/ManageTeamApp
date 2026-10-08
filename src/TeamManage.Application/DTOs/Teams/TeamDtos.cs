using TeamManage.Domain.Enums;

namespace TeamManage.Application.DTOs.Teams;

public class PositionDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

public class TeamPlayerDto
{
    public Guid TeamPlayerId { get; set; }

    public Guid PlayerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? JerseyNumber { get; set; }

    /// <summary>
    /// Populated only immediately after a brand-new player account was created
    /// as a side-effect of adding them to a panel, so the manager can relay it
    /// to the player. The player should change it on first login.
    /// </summary>
    public string? GeneratedTemporaryPassword { get; set; }
}

/// <summary>
/// Lightweight projection used for list views.
/// </summary>
public class TeamSummaryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    public int SquadSize { get; set; }

    public int PlayerCount { get; set; }
}

/// <summary>
/// Full projection used for team detail views, including panel and formation.
/// </summary>
public class TeamDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    public int SquadSize { get; set; }

    public Guid ManagerId { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public List<TeamPlayerDto> Players { get; set; } = new();

    public List<PositionDto> Positions { get; set; } = new();
}

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    public int SquadSize { get; set; }

    /// <summary>
    /// Optional custom position names, in formation order. If omitted, generic
    /// "Position 1..N" names are generated based on <see cref="SquadSize"/>.
    /// </summary>
    public List<string>? PositionNames { get; set; }
}

public class AddPlayerRequest
{
    /// <summary>
    /// Email of an existing player account to add to the panel. If no account
    /// exists with this email, one is created using <see cref="FullName"/>
    /// and a temporary password the player must change on first login.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int? JerseyNumber { get; set; }
}
