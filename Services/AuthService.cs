using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PokeBuilder.Server.Authentication.Options;
using PokeBuilder.Server.Authentication.Schemes;
using PokeBuilder.Server.Authentication.Tokens;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Auth;
using PokeBuilder.Server.Security;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public class AuthService(
    AppDbContext context,
    ITokenService tokenService,
    IOptionsMonitor<CustomAuthOptions> authOptionsMonitor,
    IPasswordBreachChecker breachChecker,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    private CustomAuthOptions Options => authOptionsMonitor.Get(CustomAuthSchemeDefaults.SchemeName);

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var passwordError = PasswordRules.Validate(request.Password);
        if (passwordError is not null)
            return ServiceResult<AuthResponse>.Fail(passwordError);

        if (Options.EnablePwnedPasswordCheck && await breachChecker.IsBreachedAsync(request.Password))
            return ServiceResult<AuthResponse>.Fail("This password is known from data breaches. Please choose a different one.");

        var emailNormalized = request.Email.ToLower().Trim();

        if (await context.Users.AnyAsync(u => u.Email == emailNormalized))
            return ServiceResult<AuthResponse>.Fail("An account with that email already exists.");

        if (await context.Users.AnyAsync(u => u.Username == request.Username))
            return ServiceResult<AuthResponse>.Fail("That username is already taken.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailNormalized,
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AccessFailedCount = 0,
            LockoutEnd = null
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(await BuildAuthResponseAsync(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var identifier = request.EmailOrUsername.Trim().ToLower();

        var user = identifier.Contains('@')
            ? await context.Users.FirstOrDefaultAsync(u => u.Email == identifier)
            : await context.Users.FirstOrDefaultAsync(u => u.Username == identifier);

        if (user is null)
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.", 401);

        if (user.LockoutEnd is { } until && until > DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Too many failed sign-in attempts. Please try again later.",
                429);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await RegisterFailedLoginAsync(user);
            return ServiceResult<AuthResponse>.Fail("Invalid credentials.", 401);
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(await BuildAuthResponseAsync(user));
    }

    public async Task<ServiceResult<AuthResponse>> RefreshAsync(RefreshRequest request)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);

        var existing = await context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (existing is null)
            return ServiceResult<AuthResponse>.Fail("Invalid refresh token.", 401);

        if (existing.RevokedAt is not null)
        {
            logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all sessions.", existing.UserId);
            await RevokeAllRefreshTokensForUserAsync(existing.UserId);
            return ServiceResult<AuthResponse>.Fail("Session invalidated. Please sign in again.", 401);
        }

        if (existing.ExpiresAt <= DateTime.UtcNow)
            return ServiceResult<AuthResponse>.Fail("Refresh token expired.", 401);

        var user = existing.User;

        existing.RevokedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(await BuildAuthResponseAsync(user));
    }

    public async Task<ServiceResult> LogoutAsync(LogoutRequest request)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var token = await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (token is null)
            return ServiceResult.Ok();

        token.RevokedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private async Task RegisterFailedLoginAsync(User user)
    {
        user.AccessFailedCount++;
        if (user.AccessFailedCount >= Options.MaxAccessFailedAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(Options.LockoutMinutes);
            user.AccessFailedCount = 0;
            logger.LogWarning("User {UserId} locked out until {Until}.", user.Id, user.LockoutEnd);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    private async Task RevokeAllRefreshTokensForUserAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var plain = CreatePlainRefreshToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = RefreshTokenHasher.Hash(plain),
            ExpiresAt = DateTime.UtcNow.AddDays(Options.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            UserAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
            CreatedIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };

        context.RefreshTokens.Add(entity);
        await context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            RefreshToken = plain,
            User = UserResponse.FromUser(user)
        };
    }

    private static string CreatePlainRefreshToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return WebEncoders.Base64UrlEncode(buffer);
    }
}
