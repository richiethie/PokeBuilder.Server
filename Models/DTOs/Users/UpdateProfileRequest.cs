using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Users;

public class UpdateProfileRequest
{
    [RegularExpression(@"^[a-z0-9_.]{3,20}$", ErrorMessage = "Username must be 3–20 characters: lowercase letters, numbers, underscores, and dots only.")]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
