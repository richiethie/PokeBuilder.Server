namespace PokeBuilder.Server.Models.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Returned only on login, register, and refresh. Omitted or null after profile-only token refresh.
    /// </summary>
    public string? RefreshToken { get; set; }

    public UserResponse User { get; set; } = new();
}
