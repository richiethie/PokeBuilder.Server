using System.ComponentModel.DataAnnotations;

namespace PokeBuilder.Server.Models.DTOs.Teams;

public class SaveTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string GameKey { get; set; } = string.Empty;

    /// <summary>
    /// Must be exactly 6 elements. Null entries represent empty slots.
    /// </summary>
    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    public int?[] PokemonIds { get; set; } = new int?[6];
}
