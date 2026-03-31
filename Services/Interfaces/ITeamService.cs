using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Teams;

namespace PokeBuilder.Server.Services.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamResponse>> GetAllAsync(Guid userId);
    Task<ServiceResult<TeamResponse>> CreateAsync(Guid userId, SaveTeamRequest request);
    Task<ServiceResult<TeamResponse>> UpdateAsync(Guid userId, Guid teamId, SaveTeamRequest request);
    Task<ServiceResult> DeleteAsync(Guid userId, Guid teamId);
}
