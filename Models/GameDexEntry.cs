namespace PokeBuilder.Server.Models;

/// <summary>
/// Records which Pokémon appear in which game's regional Pokédex, and their dex number within that game.
/// </summary>
public class GameDexEntry
{
    public string GameKey { get; set; } = string.Empty;
    public int PokemonId { get; set; }
    public int DexNumber { get; set; }

    public Game Game { get; set; } = null!;
    public PokemonEntry Pokemon { get; set; } = null!;
}
