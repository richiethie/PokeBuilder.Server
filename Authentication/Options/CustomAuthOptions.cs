using Microsoft.AspNetCore.Authentication;

namespace PokeBuilder.Server.Authentication.Options;

public class CustomAuthOptions : AuthenticationSchemeOptions
{
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PokeBuilder";
    public string Audience { get; set; } = "PokeBuilder";
    public int TokenExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// When true, requests that include the X-Dev-Bypass header are automatically
    /// authenticated as a known dev user. Should only be enabled in Development.
    /// </summary>
    public bool AllowDevBypass { get; set; } = false;

    public string DevBypassUserId { get; set; } = "00000000-0000-0000-0000-000000000001";
    public string DevBypassEmail { get; set; } = "dev@pokebuilder.local";
    public string DevBypassUsername { get; set; } = "developer";
}
