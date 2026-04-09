using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Models.Common;

namespace PokeBuilder.Server.Controllers.Base;

/// <summary>
/// Base controller for public endpoints that do not require authentication.
/// Provides the same ServiceResult → IActionResult mapping as AuthorizedController.
/// </summary>
[ApiController]
public abstract class PublicController : ControllerBase
{
    protected IActionResult FromResult<T>(ServiceResult<T> result) => result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { StatusCode: 404 } => NotFound(new { message = result.Error }),
        _ => BadRequest(new { message = result.Error })
    };
}
