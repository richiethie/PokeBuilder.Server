using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [RegularExpression(@"^[a-z0-9_.]{3,20}$", ErrorMessage = "Username must be 3–20 characters: lowercase letters, numbers, underscores, and dots only.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
    public string Password { get; set; } = string.Empty;
}
