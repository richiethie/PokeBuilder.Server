using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Authentication.Schemes;
using PokeBuilder.Server.Models.Common;

namespace PokeBuilder.Server.Controllers.Base;

[ApiController]
[Authorize(AuthenticationSchemes = CustomAuthSchemeDefaults.SchemeName)]
public abstract class AuthorizedController : ControllerBase
{
    /// <summary>The authenticated user's ID, extracted from the JWT sub claim.</summary>
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    protected string CurrentUserEmail =>
        User.FindFirstValue(JwtRegisteredClaimNames.Email)!;

    protected string CurrentUserUsername =>
        User.FindFirstValue("username")!;

    /// <summary>
    /// Maps a generic ServiceResult to the appropriate HTTP response.
    /// Controllers call this instead of manually checking IsSuccess and StatusCode.
    /// </summary>
    protected IActionResult FromResult<T>(ServiceResult<T> result) => result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { StatusCode: 401 } => Unauthorized(new { message = result.Error }),
        { StatusCode: 403 } => Forbid(),
        { StatusCode: 404 } => NotFound(new { message = result.Error }),
        _ => BadRequest(new { message = result.Error })
    };

    protected IActionResult FromResult(ServiceResult result) => result switch
    {
        { IsSuccess: true } => NoContent(),
        { StatusCode: 401 } => Unauthorized(new { message = result.Error }),
        { StatusCode: 403 } => Forbid(),
        { StatusCode: 404 } => NotFound(new { message = result.Error }),
        _ => BadRequest(new { message = result.Error })
    };
}
