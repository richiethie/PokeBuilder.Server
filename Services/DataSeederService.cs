using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public class DataSeederService(AppDbContext context) : IDataSeederService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<ServiceResult<SeedSummary>> SeedGameAsync(string gameKey)
    {
        // ── Load embedded JSON files ──────────────────────────────────────────
        var allGames = ReadEmbedded<List<GameJson>>("games.json");
        var allPokemon = ReadEmbedded<List<PokemonJson>>("pokemon.json");
        var dexIds = ReadEmbedded<List<int>>($"gameDexes/{gameKey}.json");

        if (allGames is null || allPokemon is null || dexIds is null)
            return ServiceResult<SeedSummary>.Fail($"Seed data files not found for game '{gameKey}'.");

        var gameJson = allGames.FirstOrDefault(g => g.Key == gameKey);
        if (gameJson is null)
            return ServiceResult<SeedSummary>.Fail($"Game '{gameKey}' not found in games.json.");

        // gameDetails is optional — only populated for games with detail data
        var details = ReadEmbedded<Dictionary<string, GameDetailJson>>($"gameDetails/{gameKey}.json");

        var pokemonLookup = allPokemon.ToDictionary(p => p.Id);

        int pokemonInserted = 0, dexInserted = 0, detailsInserted = 0, learnsetInserted = 0;

        // ── 1. Upsert Game ────────────────────────────────────────────────────
        var existingGame = await context.Games.FindAsync(gameKey);
        if (existingGame is null)
        {
            context.Games.Add(new Game
            {
                Key = gameJson.Key,
                Name = gameJson.Name,
                Generation = gameJson.Generation
            });
            await context.SaveChangesAsync();
        }

        // ── 2. Upsert Pokémon ─────────────────────────────────────────────────
        // Only seed the Pokémon that appear in this game's dex
        var existingPokemonIds = await context.Pokemon
            .Where(p => dexIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToHashSetAsync();

        var newPokemon = dexIds
            .Where(id => !existingPokemonIds.Contains(id) && pokemonLookup.ContainsKey(id))
            .Select(id =>
            {
                var p = pokemonLookup[id];
                return new PokemonEntry
                {
                    Id = p.Id,
                    Name = p.Name,
                    Types = p.Types,
                    StatHp = p.BaseStats.Hp,
                    StatAttack = p.BaseStats.Attack,
                    StatDefense = p.BaseStats.Defense,
                    StatSpAttack = p.BaseStats.SpAttack,
                    StatSpDefense = p.BaseStats.SpDefense,
                    StatSpeed = p.BaseStats.Speed
                };
            })
            .ToList();

        if (newPokemon.Count > 0)
        {
            context.Pokemon.AddRange(newPokemon);
            await context.SaveChangesAsync();
            pokemonInserted = newPokemon.Count;
        }

        // ── 3. Upsert GameDex entries ─────────────────────────────────────────
        var existingDexPairs = await context.GameDex
            .Where(d => d.GameKey == gameKey)
            .Select(d => d.PokemonId)
            .ToHashSetAsync();

        var newDexEntries = dexIds
            .Select((pokemonId, index) => new { pokemonId, index })
            .Where(x => !existingDexPairs.Contains(x.pokemonId) && pokemonLookup.ContainsKey(x.pokemonId))
            .Select(x => new GameDexEntry
            {
                GameKey = gameKey,
                PokemonId = x.pokemonId,
                DexNumber = x.index + 1
            })
            .ToList();

        if (newDexEntries.Count > 0)
        {
            context.GameDex.AddRange(newDexEntries);
            await context.SaveChangesAsync();
            dexInserted = newDexEntries.Count;
        }

        // ── 4. Upsert GamePokemonInfo + Learnsets ─────────────────────────────
        if (details is not null)
        {
            var existingDetailPairs = await context.GamePokemonInfo
                .Where(d => d.GameKey == gameKey)
                .Select(d => d.PokemonId)
                .ToHashSetAsync();

            var existingLearnsetPairs = await context.Learnsets
                .Where(l => l.GameKey == gameKey)
                .Select(l => l.PokemonId)
                .ToHashSetAsync();

            // Build name → id lookup (slug from JSON key, e.g. "bulbasaur" → 1)
            var nameToId = allPokemon.ToDictionary(
                p => p.Name.ToLowerInvariant().Replace(" ", "-").Replace(".", "").Replace("'", ""),
                p => p.Id
            );

            foreach (var (slug, detail) in details)
            {
                if (!nameToId.TryGetValue(slug, out var pokemonId))
                    continue;

                if (!dexIds.Contains(pokemonId))
                    continue;

                // GamePokemonInfo
                if (!existingDetailPairs.Contains(pokemonId))
                {
                    context.GamePokemonInfo.Add(new GamePokemonInfo
                    {
                        GameKey = gameKey,
                        PokemonId = pokemonId,
                        ObtainMethods = detail.ObtainMethods,
                        Locations = detail.Locations,
                        Notes = detail.Notes
                    });
                    detailsInserted++;
                }

                // Learnsets (only insert if no learnsets exist for this pokémon + game)
                if (!existingLearnsetPairs.Contains(pokemonId) && detail.Moves is not null)
                {
                    var entries = new List<Learnset>();

                    foreach (var move in detail.Moves.LevelUp ?? [])
                        entries.Add(new Learnset { GameKey = gameKey, PokemonId = pokemonId, MoveName = move.Name, LearnMethod = "levelUp", Level = move.Level });

                    foreach (var moveName in detail.Moves.Tm ?? [])
                        entries.Add(new Learnset { GameKey = gameKey, PokemonId = pokemonId, MoveName = moveName, LearnMethod = "tm" });

                    foreach (var moveName in detail.Moves.Tutor ?? [])
                        entries.Add(new Learnset { GameKey = gameKey, PokemonId = pokemonId, MoveName = moveName, LearnMethod = "tutor" });

                    context.Learnsets.AddRange(entries);
                    learnsetInserted += entries.Count;
                }
            }

            await context.SaveChangesAsync();
        }

        return ServiceResult<SeedSummary>.Ok(new SeedSummary(
            pokemonInserted, dexInserted, detailsInserted, learnsetInserted));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static T? ReadEmbedded<T>(string relativePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Embedded resource names: PokeBuilder.Server.SeedData.subpath.file
        // Path separators become dots; convert relative path accordingly
        var resourceSuffix = relativePath.Replace("/", ".").Replace("\\", ".");
        var resourceName = $"PokeBuilder.Server.SeedData.{resourceSuffix}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return default;

        return JsonSerializer.Deserialize<T>(stream, JsonOpts);
    }

    // ── Private JSON DTOs (mirror the JSON structure) ─────────────────────────

    private record GameJson(string Key, string Name, int Generation);

    private record PokemonJson(int Id, string Name, string[] Types, BaseStatsJson BaseStats);

    private record BaseStatsJson(int Hp, int Attack, int Defense, int SpAttack, int SpDefense, int Speed);

    private record GameDetailJson(
        string[] ObtainMethods,
        string[] Locations,
        string? Notes,
        MovesJson? Moves
    );

    private record MovesJson(
        LevelUpMoveJson[]? LevelUp,
        string[]? Tm,
        string[]? Tutor
    );

    private record LevelUpMoveJson(int Level, string Name);
}
