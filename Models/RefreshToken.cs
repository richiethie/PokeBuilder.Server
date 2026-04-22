namespace PokeBuilder.Server.Models;

/// <summary>
/// Rotating refresh token. Only <see cref="TokenHash"/> is stored; the plaintext token is returned once to the client.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>SHA-256 hash of the plaintext refresh token (Base64).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public string? UserAgent { get; set; }
    public string? CreatedIp { get; set; }
}
