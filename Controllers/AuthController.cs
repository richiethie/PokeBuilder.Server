using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PokeBuilder.Server.Authentication.Schemes;
using PokeBuilder.Server.Models.DTOs.Auth;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return StatusCode(201, result.Data);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (!result.IsSuccess)
        {
            if (result.StatusCode == 429)
                return StatusCode(429, new { message = result.Error });
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await authService.RefreshAsync(request);

        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth-logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await authService.LogoutAsync(request);
        return NoContent();
    }

    /// <summary>
    /// Returns the currently authenticated user's profile, decoded from the JWT.
    /// Does not hit the database — useful for re-hydrating the frontend session.
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = CustomAuthSchemeDefaults.SchemeName)]
    public IActionResult Me()
    {
        return Ok(new UserResponse
        {
            Id = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email)!,
            Username = User.FindFirstValue("username")!
        });
    }
}
