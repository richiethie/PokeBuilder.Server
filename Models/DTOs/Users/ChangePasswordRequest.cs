using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Users;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(12, ErrorMessage = "New password must be at least 12 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
