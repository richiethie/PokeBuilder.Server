using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PokeBuilder.Server.Authentication.Options;
using PokeBuilder.Server.Authentication.Schemes;
using PokeBuilder.Server.Models;

namespace PokeBuilder.Server.Authentication.Tokens;

public class JwtTokenService : ITokenService
{
    private readonly CustomAuthOptions _options;

    public JwtTokenService(IOptionsMonitor<CustomAuthOptions> optionsMonitor)
    {
        _options = optionsMonitor.Get(CustomAuthSchemeDefaults.SchemeName);
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("displayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.TokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
