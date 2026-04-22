using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Auth;

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
