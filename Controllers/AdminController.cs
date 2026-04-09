using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeBuilder.Server.Services.Interfaces;

namespace PokeBuilder.Server.Controllers;

/// <summary>
/// Developer / admin endpoints. All actions require authentication and are
/// intended for use during development and initial deployment, not user-facing.
/// </summary>
[Route("api/admin")]
[Authorize]
public class AdminController(IDataSeederService seeder) : ControllerBase
{
    /// <summary>
    /// Seeds all game data for the specified game key from embedded JSON files.
    /// Safe to call multiple times — existing rows are skipped (idempotent).
    /// Example: POST /api/admin/seed/firered
    /// </summary>
    [HttpPost("seed/{gameKey}")]
    public async Task<IActionResult> SeedGame(string gameKey)
    {
        var result = await seeder.SeedGameAsync(gameKey);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            message = $"Seeded '{gameKey}' successfully.",
            summary = result.Data
        });
    }
}
