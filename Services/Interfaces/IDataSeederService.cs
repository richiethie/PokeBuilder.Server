using PokeBuilder.Server.Models.Common;

namespace PokeBuilder.Server.Services.Interfaces;

public interface IDataSeederService
{
    /// <summary>
    /// Seeds all base Pokémon and game data for the given game key.
    /// Safe to call multiple times — existing rows are skipped.
    /// </summary>
    Task<ServiceResult<SeedSummary>> SeedGameAsync(string gameKey);
}

public record SeedSummary(
    int PokemonInserted,
    int DexEntriesInserted,
    int DetailsInserted,
    int LearnsetEntriesInserted
);
