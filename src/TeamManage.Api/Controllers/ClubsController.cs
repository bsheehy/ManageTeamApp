using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Clubs;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.ClubManager))]
public class ClubsController : ApiControllerBase
{
    private readonly IClubService _clubService;

    public ClubsController(IClubService clubService)
    {
        _clubService = clubService;
    }

    /// <summary>
    /// Gets the clubs managed by the current Club Manager.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ClubDto>>> GetMyClubs(CancellationToken ct)
    {
        var result = await _clubService.GetClubsForManagerAsync(CurrentUserId, ct);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new club owned by the current Club Manager.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClubDto>> Create(CreateClubRequest request, CancellationToken ct)
    {
        var result = await _clubService.CreateClubAsync(CurrentUserId, request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return Ok(result.Data);
    }
}
