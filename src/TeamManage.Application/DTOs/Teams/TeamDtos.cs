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

    public Guid ClubId { get; set; }

    public string ClubName { get; set; } = string.Empty;

    /// <summary>True when the current user manages this team (team manager or club manager).</summary>
    public bool CanManage { get; set; }
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

    public Guid? ManagerId { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public Guid ClubId { get; set; }

    public string ClubName { get; set; } = string.Empty;

    /// <summary>True when the current user can manage players and matches for this team.</summary>
    public bool CanManage { get; set; }

    /// <summary>True when the current user is the club manager and can assign the team manager.</summary>
    public bool CanAssignManager { get; set; }

    public List<TeamPlayerDto> Players { get; set; } = new();

    public List<PositionDto> Positions { get; set; } = new();
}

public class CreateTeamRequest
{
    public Guid? ManagerId { get; set; }

    public Guid ClubId { get; set; }

    public string Name { get; set; } = string.Empty;

    public SportType Sport { get; set; }

    public int SquadSize { get; set; }

    /// <summary>
    /// Optional custom position names, in formation order. If omitted, generic
    /// "Position 1..N" names are generated based on <see cref="SquadSize"/>.
    /// </summary>
    public List<string>? PositionNames { get; set; }
}

public class AssignManagerRequest
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}

public class TeamManagerDto
{
    public Guid ManagerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>True when a new account was created and an invite email was sent.</summary>
    public bool InviteSent { get; set; }
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
