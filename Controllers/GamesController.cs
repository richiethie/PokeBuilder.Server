using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Controllers.Base;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Controllers;

[Route("api/games")]
public class GamesController(IGameService gameService) : PublicController
{
    /// <summary>GET /api/games — list all games ordered by generation.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await gameService.GetAllGamesAsync());

    /// <summary>GET /api/games/{key}/pokemon — full Pokédex for a game, ordered by regional dex number.</summary>
    [HttpGet("{key}/pokemon")]
    public async Task<IActionResult> GetDex(string key)
    {
        var result = await gameService.GetDexAsync(key);
        return FromResult(result);
    }

    /// <summary>GET /api/games/{key}/pokemon/{slug} — summary + game-specific detail for one Pokémon.</summary>
    [HttpGet("{key}/pokemon/{slug}")]
    public async Task<IActionResult> GetPokemonDetail(string key, string slug)
    {
        var result = await gameService.GetPokemonDetailAsync(key, slug);
        return FromResult(result);
    }
}
