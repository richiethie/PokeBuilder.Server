namespace PokeBuilder.Server.Models.DTOs.Games;

public class LevelUpMoveResponse
{
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PokemonMovesResponse
{
    public List<LevelUpMoveResponse> LevelUp { get; set; } = [];
    public List<string> Tm { get; set; } = [];
    public List<string> Tutor { get; set; } = [];
}

public class PokemonGameInfoResponse
{
    public string[] ObtainMethods { get; set; } = [];
    public string[] Locations { get; set; } = [];
    public string? Notes { get; set; }
    public PokemonMovesResponse Moves { get; set; } = new();
}

/// <summary>
/// Full Pokémon detail response: base summary + optional game-specific info.
/// GameInfo is null when we don't have detail data for this Pokémon in this game yet.
/// </summary>
public class PokemonDetailResponse : PokemonSummaryResponse
{
    public PokemonGameInfoResponse? GameInfo { get; set; }
}
