namespace PokeBuilder.Server.Authentication.Tokens;

/// <summary>
/// Abstraction for token validation logic. Swap the registered implementation
/// to support different token types (JWT, API keys, OAuth introspection, etc.)
/// without changing the authentication handler.
/// </summary>
public interface ITokenValidationProvider
{
    Task<TokenValidationResult> ValidateAsync(string token);
}
