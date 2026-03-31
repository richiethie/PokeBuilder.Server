using System.Security.Claims;

namespace PokeBuilder.Server.Authentication.Tokens;

public class TokenValidationResult
{
    public bool IsValid { get; private init; }
    public ClaimsPrincipal? Principal { get; private init; }
    public string? Error { get; private init; }

    public static TokenValidationResult Success(ClaimsPrincipal principal) =>
        new() { IsValid = true, Principal = principal };

    public static TokenValidationResult Failure(string error) =>
        new() { IsValid = false, Error = error };
}
