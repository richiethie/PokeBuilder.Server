namespace PokeBuilder.Server.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Failed password attempts since last success or lockout reset.</summary>
    public int AccessFailedCount { get; set; }

    /// <summary>When non-null and in the future, sign-in is blocked until this instant.</summary>
    public DateTime? LockoutEnd { get; set; }
}
