using System.Text.RegularExpressions;

namespace PokeBuilder.Server.Security;

public static class PasswordRules
{
    public const int MinLength = 12;
    public const int MaxLength = 128;

    /// <summary>Returns null if valid, otherwise a user-facing error message.</summary>
    public static string? Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Password is required.";

        if (password.Length < MinLength)
            return $"Password must be at least {MinLength} characters.";

        if (password.Length > MaxLength)
            return $"Password must be at most {MaxLength} characters.";

        // Disallow whitespace-only passwords
        if (!Regex.IsMatch(password, @"\S"))
            return "Password must contain at least one non-whitespace character.";

        return null;
    }
}
