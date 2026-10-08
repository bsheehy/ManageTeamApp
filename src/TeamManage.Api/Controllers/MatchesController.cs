using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Matches;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class MatchesController : ApiControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    /// <summary>
    /// Gets matches relevant to the current user: fixtures for a managed team,
    /// or matches the current Player has been invited to.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MatchSummaryDto>>> GetMyMatches([FromQuery] Guid? teamId, CancellationToken ct)
    {
        var isManager = User.IsInRole(nameof(UserRole.Manager));
        if (isManager && teamId.HasValue)
        {
            var teamResult = await _matchService.GetMatchesForTeamAsync(teamId.Value, ct);
            return Ok(teamResult.Data);
        }

        var result = await _matchService.GetMatchesForPlayerAsync(CurrentUserId, ct);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets full detail for a single match, including the full squad list and
    /// each player's attendance status.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _matchService.GetMatchDetailAsync(id, ct);
        if (!result.Succeeded)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Schedules a new match/fixture for a team. Manager (and owner of the team) only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<ActionResult<MatchDto>> Create(CreateMatchRequest request, CancellationToken ct)
    {
        var result = await _matchService.CreateMatchAsync(CurrentUserId, request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Sets the current Player's own attendance response (confirm/decline) for a match.
    /// </summary>
    [HttpPut("{id:guid}/attendance")]
    [Authorize(Roles = nameof(UserRole.Player))]
    public async Task<IActionResult> SetAttendance(Guid id, AttendanceResponseRequest request, CancellationToken ct)
    {
        var result = await _matchService.SetAttendanceAsync(id, CurrentUserId, request.Status, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return NoContent();
    }

    /// <summary>
    /// Allocates players to starting positions and substitutes for a match.
    /// Manager (and owner of the team) only.
    /// </summary>
    [HttpPut("{id:guid}/selections")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<ActionResult<MatchDto>> AllocateSelections(Guid id, AllocateSelectionsRequest request, CancellationToken ct)
    {
        var result = await _matchService.AllocateSelectionsAsync(id, CurrentUserId, request.Allocations, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return Ok(result.Data);
    }
}
