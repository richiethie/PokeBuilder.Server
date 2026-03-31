using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Controllers.Base;
using PokeBuilder.Server.Models.DTOs.Teams;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Controllers;

[Route("api/teams")]
public class TeamsController(ITeamService teamService) : AuthorizedController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await teamService.GetAllAsync(CurrentUserId);
        return Ok(teams);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveTeamRequest request)
    {
        var result = await teamService.CreateAsync(CurrentUserId, request);

        if (!result.IsSuccess)
            return FromResult(result);

        return StatusCode(201, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveTeamRequest request)
    {
        return FromResult(await teamService.UpdateAsync(CurrentUserId, id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return FromResult(await teamService.DeleteAsync(CurrentUserId, id));
    }
}
