using PokeBuilder.Server.Models;

namespace PokeBuilder.Server.Models.DTOs.Games;

public class BaseStatsResponse
{
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpAttack { get; set; }
    public int SpDefense { get; set; }
    public int Speed { get; set; }
}

public class PokemonSummaryResponse
{
    public int Id { get; set; }
    /// <summary>Regional dex number for this game (e.g. Johto #001 for Chikorita in HG/SS).</summary>
    public int DexNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] Types { get; set; } = [];
    public BaseStatsResponse BaseStats { get; set; } = new();

    public static PokemonSummaryResponse FromEntry(PokemonEntry p, int dexNumber) => new()
    {
        Id = p.Id,
        DexNumber = dexNumber,
        Name = p.Name,
        Types = p.Types,
        BaseStats = new BaseStatsResponse
        {
            Hp = p.StatHp,
            Attack = p.StatAttack,
            Defense = p.StatDefense,
            SpAttack = p.StatSpAttack,
            SpDefense = p.StatSpDefense,
            Speed = p.StatSpeed
        }
    };
}
