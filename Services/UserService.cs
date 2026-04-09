using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Authentication.Tokens;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Auth;
using PokeBuilder.Server.Models.DTOs.Users;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public class UserService(AppDbContext context, ITokenService tokenService) : IUserService
{
    public async Task<ServiceResult<AuthResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            return ServiceResult<AuthResponse>.Fail("User not found.", 404);

        if (request.Username is not null)
        {
            var usernameTaken = await context.Users
                .AnyAsync(u => u.Username == request.Username && u.Id != userId);
            if (usernameTaken)
                return ServiceResult<AuthResponse>.Fail("That username is already taken.");

            user.Username = request.Username;
        }

        if (request.Email is not null)
        {
            var emailNormalized = request.Email.ToLower().Trim();
            var emailTaken = await context.Users
                .AnyAsync(u => u.Email == emailNormalized && u.Id != userId);
            if (emailTaken)
                return ServiceResult<AuthResponse>.Fail("An account with that email already exists.");

            user.Email = emailNormalized;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Return a fresh token so the JWT reflects any username/email changes immediately.
        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            User = UserResponse.FromUser(user)
        });
    }

    public async Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            return ServiceResult.Fail("User not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAccountAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            return ServiceResult.Fail("User not found.", 404);

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}
