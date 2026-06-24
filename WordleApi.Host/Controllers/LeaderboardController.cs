using Microsoft.AspNetCore.Mvc;
using WordleApi.Host.Data;
using WordleApi.Host.Models;

namespace WordleApi.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardRepository _leaderboardRepository;

    public LeaderboardController(ILeaderboardRepository leaderboardRepository)
    {
        _leaderboardRepository = leaderboardRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<LeaderboardEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopScores(
        [FromQuery] string period = "all",
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        if (limit is < 1 or > 100)
            limit = 10;

        var entries = await _leaderboardRepository.GetTopScoresAsync(period, limit, ct);
        return Ok(entries);
    }

    [HttpGet("players/{name}")]
    [ProducesResponseType(typeof(PlayerStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerStats(string name, CancellationToken ct)
    {
        var stats = await _leaderboardRepository.GetPlayerStatsAsync(name, ct);
        if (stats is null)
            return NotFound();

        return Ok(stats);
    }
}
