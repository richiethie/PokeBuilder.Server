using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Games;

namespace PokeBuilder.Server.Services.Interfaces;

public interface IGameService
{
    Task<List<GameResponse>> GetAllGamesAsync();
    Task<ServiceResult<List<PokemonSummaryResponse>>> GetDexAsync(string gameKey);
    Task<ServiceResult<PokemonDetailResponse>> GetPokemonDetailAsync(string gameKey, string slug);
}
