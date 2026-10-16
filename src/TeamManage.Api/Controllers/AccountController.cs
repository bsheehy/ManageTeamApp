using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Auth;
using TeamManage.Application.Interfaces.Services;

namespace TeamManage.Api.Controllers;

[Route("api/account")]
[Authorize]
public class AccountController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Sets a new password for the current user (used after signing in via an invite link).
    /// </summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _authService.ChangePasswordAsync(CurrentUserId, request.NewPassword, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return NoContent();
    }
}
