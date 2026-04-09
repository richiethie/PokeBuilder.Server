namespace PokeBuilder.Server.Models;

/// <summary>
/// Game-specific details for a Pokémon: how to obtain it, where to find it, and optional notes.
/// Move lists are stored separately in the Learnset table.
/// </summary>
public class GamePokemonInfo
{
    public int Id { get; set; }
    public string GameKey { get; set; } = string.Empty;
    public int PokemonId { get; set; }
    public string[] ObtainMethods { get; set; } = [];
    public string[] Locations { get; set; } = [];
    public string? Notes { get; set; }

    public Game Game { get; set; } = null!;
    public PokemonEntry Pokemon { get; set; } = null!;
}
