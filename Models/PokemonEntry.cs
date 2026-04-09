namespace PokeBuilder.Server.Models;

public class PokemonEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] Types { get; set; } = [];

    public int StatHp { get; set; }
    public int StatAttack { get; set; }
    public int StatDefense { get; set; }
    public int StatSpAttack { get; set; }
    public int StatSpDefense { get; set; }
    public int StatSpeed { get; set; }

    public ICollection<GameDexEntry> DexEntries { get; set; } = [];
    public ICollection<GamePokemonInfo> GameDetails { get; set; } = [];
    public ICollection<Learnset> Learnsets { get; set; } = [];
}
