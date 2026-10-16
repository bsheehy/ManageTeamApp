using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Teams;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TeamsController : ApiControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Gets the teams relevant to the current user: managed teams for a Manager,
    /// or panel memberships for a Player.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TeamSummaryDto>>> GetMyTeams(CancellationToken ct)
    {
        var result = await _teamService.GetTeamsForUserAsync(CurrentUserId, ct);

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets full detail for a single team, including panel and formation.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _teamService.GetTeamDetailAsync(id, CurrentUserId, ct);
        if (!result.Succeeded)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new named panel/team. Manager role only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.ClubManager))]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request, CancellationToken ct)
    {
        var result = await _teamService.CreateTeamAsync(CurrentUserId, request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Assigns the team's manager. Club manager of the team's club only.
    /// </summary>
    [HttpPut("{id:guid}/manager")]
    [Authorize(Roles = nameof(UserRole.ClubManager))]
    public async Task<ActionResult<TeamManagerDto>> AssignManager(Guid id, AssignManagerRequest request, CancellationToken ct)
    {
        var result = await _teamService.AssignTeamManagerAsync(id, CurrentUserId, request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Adds a player to the team's panel. Team manager or club manager only.
    /// </summary>
    [HttpPost("{id:guid}/players")]
    [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.TeamManager)}")]
    public async Task<ActionResult<TeamPlayerDto>> AddPlayer(Guid id, AddPlayerRequest request, CancellationToken ct)
    {
        var result = await _teamService.AddPlayerToTeamAsync(id, CurrentUserId, request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Removes a player from the team's panel. Manager (and owner of the team) only.
    /// </summary>
    [HttpDelete("{id:guid}/players/{teamPlayerId:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.TeamManager)}")]
    public async Task<IActionResult> RemovePlayer(Guid id, Guid teamPlayerId, CancellationToken ct)
    {
        var result = await _teamService.RemovePlayerFromTeamAsync(id, CurrentUserId, teamPlayerId, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return NoContent();
    }
}
