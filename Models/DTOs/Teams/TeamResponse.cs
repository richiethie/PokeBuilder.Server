namespace PokeBuilder.Server.Models.DTOs.Teams;

public class TeamResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GameKey { get; set; } = string.Empty;
    public int?[] PokemonIds { get; set; } = new int?[6];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static TeamResponse FromTeam(Models.Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        GameKey = team.GameKey,
        PokemonIds = team.PokemonIds,
        CreatedAt = team.CreatedAt,
        UpdatedAt = team.UpdatedAt
    };
}
