using Microsoft.AspNetCore.Mvc;
using WordleApi.Host.Data;
using WordleApi.Host.Models;

namespace WordleApi.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(ILeaderboardRepository leaderboardRepository, ILogger<LeaderboardController> logger)
    {
        _leaderboardRepository = leaderboardRepository;
        _logger = logger;
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

        _logger.LogInformation("Fetching top {Limit} scores for period {Period}", limit, period);
        var entries = await _leaderboardRepository.GetTopScoresAsync(period, limit, ct);
        _logger.LogInformation("Returned {Count} leaderboard entries for period {Period}", entries.Count(), period);
        return Ok(entries);
    }

    [HttpGet("players/{name}")]
    [ProducesResponseType(typeof(PlayerStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerStats(string name, CancellationToken ct)
    {
        _logger.LogInformation("Fetching stats for player {PlayerName}", name);
        var stats = await _leaderboardRepository.GetPlayerStatsAsync(name, ct);
        if (stats is null)
        {
            _logger.LogWarning("Player {PlayerName} not found", name);
            return NotFound();
        }

        return Ok(stats);
    }
}
