using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Authentication.Tokens;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Auth;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public class AuthService(AppDbContext context, ITokenService tokenService) : IAuthService
{
    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var emailNormalized = request.Email.ToLower().Trim();

        if (await context.Users.AnyAsync(u => u.Email == emailNormalized))
            return ServiceResult<AuthResponse>.Fail("An account with that email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailNormalized,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var emailNormalized = request.Email.ToLower().Trim();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Fail("Invalid email or password.", 401);

        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    private AuthResponse BuildAuthResponse(User user) => new()
    {
        Token = tokenService.GenerateToken(user),
        User = UserResponse.FromUser(user)
    };
}
