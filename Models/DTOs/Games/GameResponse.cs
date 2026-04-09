using PokeBuilder.Server.Models;

namespace PokeBuilder.Server.Models.DTOs.Games;

public class GameResponse
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Generation { get; set; }

    public static GameResponse FromGame(Game g) => new()
    {
        Key = g.Key,
        Name = g.Name,
        Generation = g.Generation
    };
}
