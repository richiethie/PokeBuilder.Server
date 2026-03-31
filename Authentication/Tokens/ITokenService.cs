using PokeBuilder.Server.Models;

namespace PokeBuilder.Server.Authentication.Tokens;

/// <summary>
/// Abstraction for token generation. Used by AuthService after a successful
/// login or registration to produce a signed JWT for the client.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}
