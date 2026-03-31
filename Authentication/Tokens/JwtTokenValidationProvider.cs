using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PokeBuilder.Server.Authentication.Options;

namespace PokeBuilder.Server.Authentication.Tokens;

public class JwtTokenValidationProvider : ITokenValidationProvider
{
    private readonly CustomAuthOptions _options;

    public JwtTokenValidationProvider(IOptionsMonitor<CustomAuthOptions> optionsMonitor)
    {
        _options = optionsMonitor.Get(Schemes.CustomAuthSchemeDefaults.SchemeName);
    }

    public Task<TokenValidationResult> ValidateAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_options.JwtSecret);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParams, out _);
            return Task.FromResult(TokenValidationResult.Success(principal));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TokenValidationResult.Failure(ex.Message));
        }
    }
}
