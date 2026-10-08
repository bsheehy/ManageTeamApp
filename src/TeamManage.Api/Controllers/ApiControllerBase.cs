using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TeamManage.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// The authenticated user's id, extracted from the JWT "sub" claim.
    /// </summary>
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User id claim is missing."));

    /// <summary>
    /// Maps a failed <see cref="Application.Common.Result"/> to an appropriate
    /// HTTP response. Callers should check <c>Succeeded</c> before calling this.
    /// </summary>
    protected ActionResult ProblemFrom(string error) =>
        Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
}
