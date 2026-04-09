using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Auth;

public class LoginRequest
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
