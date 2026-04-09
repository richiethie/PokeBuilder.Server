namespace PokeBuilder.Server.Models;

public class Game
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Generation { get; set; }

    public ICollection<GameDexEntry> DexEntries { get; set; } = [];
}
