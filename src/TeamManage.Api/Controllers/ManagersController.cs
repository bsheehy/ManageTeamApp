using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Managers;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.Api.Controllers;

[Route("api/managers")]
[Authorize(Roles = nameof(UserRole.ClubManager))]
public class ManagersController : ApiControllerBase
{
    private readonly IManagerService _managerService;

    public ManagersController(IManagerService managerService)
    {
        _managerService = managerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeamManagerSummaryDto>>> GetAll(CancellationToken ct)
    {
        var result = await _managerService.GetManagersAsync(CurrentUserId, ct);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a team manager account and emails them a sign-in link.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TeamManagerSummaryDto>> Create(SaveTeamManagerRequest request, CancellationToken ct)
    {
        var result = await _managerService.CreateManagerAsync(CurrentUserId, request, ct);
        return result.Succeeded ? Ok(result.Data) : ProblemFrom(result.Error!);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TeamManagerSummaryDto>> Update(Guid id, SaveTeamManagerRequest request, CancellationToken ct)
    {
        var result = await _managerService.UpdateManagerAsync(CurrentUserId, id, request, ct);
        return result.Succeeded ? Ok(result.Data) : ProblemFrom(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _managerService.DeleteManagerAsync(CurrentUserId, id, ct);
        return result.Succeeded ? NoContent() : ProblemFrom(result.Error!);
    }

    [HttpPost("{id:guid}/resend-invite")]
    public async Task<IActionResult> ResendInvite(Guid id, CancellationToken ct)
    {
        var result = await _managerService.ResendInviteAsync(CurrentUserId, id, ct);
        return result.Succeeded ? NoContent() : ProblemFrom(result.Error!);
    }
}
