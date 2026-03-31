namespace PokeBuilder.Server.Models;

public class Team
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The game key this team was built for (e.g. "firered", "heartgold").
    /// </summary>
    public string GameKey { get; set; } = string.Empty;

    /// <summary>
    /// Six-slot ordered array of Pokémon IDs. Null entries represent empty slots.
    /// Stored as JSONB in PostgreSQL via a value converter in AppDbContext.
    /// </summary>
    public int?[] PokemonIds { get; set; } = new int?[6];

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
