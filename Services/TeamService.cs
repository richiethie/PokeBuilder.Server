using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Data;
using PokeBuilder.Server.Models;
using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Teams;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Services;

public class TeamService(AppDbContext context) : ITeamService
{
    public async Task<IEnumerable<TeamResponse>> GetAllAsync(Guid userId)
    {
        return await context.Teams
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => TeamResponse.FromTeam(t))
            .ToListAsync();
    }

    public async Task<ServiceResult<TeamResponse>> CreateAsync(Guid userId, SaveTeamRequest request)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            GameKey = request.GameKey,
            PokemonIds = request.PokemonIds,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Teams.Add(team);
        await context.SaveChangesAsync();

        return ServiceResult<TeamResponse>.Ok(TeamResponse.FromTeam(team));
    }

    public async Task<ServiceResult<TeamResponse>> UpdateAsync(Guid userId, Guid teamId, SaveTeamRequest request)
    {
        var team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null)
            return ServiceResult<TeamResponse>.Fail("Team not found.", 404);

        if (team.UserId != userId)
            return ServiceResult<TeamResponse>.Fail("You do not have permission to modify this team.", 403);

        team.Name = request.Name.Trim();
        team.GameKey = request.GameKey;
        team.PokemonIds = request.PokemonIds;
        team.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return ServiceResult<TeamResponse>.Ok(TeamResponse.FromTeam(team));
    }

    public async Task<ServiceResult> DeleteAsync(Guid userId, Guid teamId)
    {
        var team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null)
            return ServiceResult.Fail("Team not found.", 404);

        if (team.UserId != userId)
            return ServiceResult.Fail("You do not have permission to delete this team.", 403);

        context.Teams.Remove(team);
        await context.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}
