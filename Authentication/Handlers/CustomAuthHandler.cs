using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PokeBuilder.Server.Authentication.Options;
using PokeBuilder.Server.Authentication.Tokens;

namespace PokeBuilder.Server.Authentication.Handlers;

public class CustomAuthHandler(
    IOptionsMonitor<CustomAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITokenValidationProvider tokenValidationProvider)
    : AuthenticationHandler<CustomAuthOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Dev bypass — skip token validation entirely when the header is present
        if (Options.AllowDevBypass && Request.Headers.ContainsKey("X-Dev-Bypass"))
        {
            var devTicket = BuildDevTicket();
            return AuthenticateResult.Success(devTicket);
        }

        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = headerValue["Bearer ".Length..].Trim();

        var result = await tokenValidationProvider.ValidateAsync(token);

        if (!result.IsValid)
            return AuthenticateResult.Fail(result.Error ?? "Invalid token.");

        var ticket = new AuthenticationTicket(result.Principal!, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    private AuthenticationTicket BuildDevTicket()
    {
        var claims = new[]
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, Options.DevBypassUserId),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, Options.DevBypassEmail),
            new Claim("username", Options.DevBypassUsername)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, Scheme.Name);
    }
}
