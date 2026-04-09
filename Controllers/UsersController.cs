using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Authentication.Tokens;
using PokeBuilder.Server.Controllers.Base;
using PokeBuilder.Server.Models.DTOs.Users;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Controllers;

[Route("api/users")]
public class UsersController(IUserService userService) : AuthorizedController
{
    /// <summary>
    /// Updates the authenticated user's username and/or email.
    /// Returns a fresh JWT + user profile so the frontend can update its session.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await userService.UpdateProfileAsync(CurrentUserId, request);
        return FromResult(result);
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// Requires the current password to be provided for verification.
    /// </summary>
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await userService.ChangePasswordAsync(CurrentUserId, request);
        return FromResult(result);
    }

    /// <summary>
    /// Permanently deletes the authenticated user's account and all associated teams.
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await userService.DeleteAccountAsync(CurrentUserId);
        return FromResult(result);
    }
}
