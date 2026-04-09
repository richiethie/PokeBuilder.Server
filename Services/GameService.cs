using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Games;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public partial class GameService(AppDbContext context) : IGameService
{
    public async Task<List<GameResponse>> GetAllGamesAsync()
    {
        var games = await context.Games
            .OrderBy(g => g.Generation)
            .ToListAsync();

        return games.Select(GameResponse.FromGame).ToList();
    }

    public async Task<ServiceResult<List<PokemonSummaryResponse>>> GetDexAsync(string gameKey)
    {
        var exists = await context.Games.AnyAsync(g => g.Key == gameKey);
        if (!exists)
            return ServiceResult<List<PokemonSummaryResponse>>.Fail($"Game '{gameKey}' not found.", 404);

        var entries = await context.GameDex
            .Where(d => d.GameKey == gameKey)
            .OrderBy(d => d.DexNumber)
            .Include(d => d.Pokemon)
            .ToListAsync();

        return ServiceResult<List<PokemonSummaryResponse>>.Ok(
            entries.Select(d => PokemonSummaryResponse.FromEntry(d.Pokemon, d.DexNumber)).ToList());
    }

    public async Task<ServiceResult<PokemonDetailResponse>> GetPokemonDetailAsync(string gameKey, string slug)
    {
        // Resolve Pokémon by slug: find whose name matches when slugified.
        // We load all Pokémon in this game's dex and filter in memory since the
        // name column is not slugified in the DB — but the dex is at most 151–250 rows.
        var dexEntries = await context.GameDex
            .Where(d => d.GameKey == gameKey)
            .Include(d => d.Pokemon)
            .ToListAsync();

        var matchedEntry = dexEntries.FirstOrDefault(e => ToSlug(e.Pokemon.Name) == slug);

        if (matchedEntry is null)
            return ServiceResult<PokemonDetailResponse>.Fail(
                $"Pokémon '{slug}' not found in game '{gameKey}'.", 404);

        var pokemon = matchedEntry.Pokemon;

        var response = new PokemonDetailResponse
        {
            Id = pokemon.Id,
            DexNumber = matchedEntry.DexNumber,
            Name = pokemon.Name,
            Types = pokemon.Types,
            BaseStats = new BaseStatsResponse
            {
                Hp = pokemon.StatHp,
                Attack = pokemon.StatAttack,
                Defense = pokemon.StatDefense,
                SpAttack = pokemon.StatSpAttack,
                SpDefense = pokemon.StatSpDefense,
                Speed = pokemon.StatSpeed
            }
        };

        // Load game-specific info if available
        var info = await context.GamePokemonInfo
            .FirstOrDefaultAsync(i => i.GameKey == gameKey && i.PokemonId == pokemon.Id);

        if (info is not null)
        {
            var learnsets = await context.Learnsets
                .Where(l => l.GameKey == gameKey && l.PokemonId == pokemon.Id)
                .ToListAsync();

            response.GameInfo = new PokemonGameInfoResponse
            {
                ObtainMethods = info.ObtainMethods,
                Locations = info.Locations,
                Notes = info.Notes,
                Moves = new PokemonMovesResponse
                {
                    LevelUp = learnsets
                        .Where(l => l.LearnMethod == "levelUp")
                        .OrderBy(l => l.Level)
                        .Select(l => new LevelUpMoveResponse { Level = l.Level ?? 1, Name = l.MoveName })
                        .ToList(),
                    Tm = learnsets
                        .Where(l => l.LearnMethod == "tm")
                        .Select(l => l.MoveName)
                        .ToList(),
                    Tutor = learnsets
                        .Where(l => l.LearnMethod == "tutor")
                        .Select(l => l.MoveName)
                        .ToList()
                }
            };
        }

        return ServiceResult<PokemonDetailResponse>.Ok(response);
    }

    /// <summary>
    /// Mirrors the frontend toPokemonSlug: lowercase, replace non-alphanumeric runs with
    /// a hyphen, then trim leading/trailing hyphens.
    /// e.g. "Mr. Mime" → "mr-mime", "Farfetch'd" → "farfetchd" → wait, apostrophe is non-alnum
    /// so it becomes a hyphen, but is at end → trimmed? Let's trace: "Farfetch'd" →
    /// lower "farfetch'd" → replace [^a-z0-9]+ → "farfetch-d" → trim → "farfetch-d"
    /// matches the frontend output.
    /// </summary>
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    private static string ToSlug(string name) =>
        NonAlphanumericRegex().Replace(name.ToLowerInvariant(), "-").Trim('-');
}
