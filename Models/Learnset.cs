namespace PokeBuilder.Server.Models;

public class Learnset
{
    public int Id { get; set; }
    public string GameKey { get; set; } = string.Empty;
    public int PokemonId { get; set; }
    public string MoveName { get; set; } = string.Empty;

    /// <summary>"levelUp" | "tm" | "tutor"</summary>
    public string LearnMethod { get; set; } = string.Empty;

    /// <summary>Only populated for levelUp moves.</summary>
    public int? Level { get; set; }

    public Game Game { get; set; } = null!;
    public PokemonEntry Pokemon { get; set; } = null!;
}
