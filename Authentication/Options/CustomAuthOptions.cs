using Microsoft.AspNetCore.Authentication;

namespace PokeBuilder.Server.Authentication.Options;

public class CustomAuthOptions : AuthenticationSchemeOptions
{
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PokeBuilder";
    public string Audience { get; set; } = "PokeBuilder";
    /// <summary>Access JWT lifetime. Keep short when using refresh tokens.</summary>
    public int TokenExpiryMinutes { get; set; } = 15;

    /// <summary>Refresh token absolute lifetime from creation.</summary>
    public int RefreshTokenExpiryDays { get; set; } = 14;

    public int MaxAccessFailedAttempts { get; set; } = 5;

    /// <summary>Account lockout duration after too many failed logins.</summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>When true, passwords found in HIBP corpus are rejected at register / password change.</summary>
    public bool EnablePwnedPasswordCheck { get; set; } = true;

    /// <summary>
    /// When true, requests that include the X-Dev-Bypass header are automatically
    /// authenticated as a known dev user. Should only be enabled in Development.
    /// </summary>
    public bool AllowDevBypass { get; set; } = false;

    public string DevBypassUserId { get; set; } = "00000000-0000-0000-0000-000000000001";
    public string DevBypassEmail { get; set; } = "dev@pokebuilder.local";
    public string DevBypassUsername { get; set; } = "developer";
}
