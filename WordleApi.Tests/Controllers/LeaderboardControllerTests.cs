using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WordleApi.Host.Controllers;
using WordleApi.Host.Data;
using WordleApi.Host.Models;
using Xunit;

namespace WordleApi.Tests.Controllers;

public class LeaderboardControllerTests
{
    private readonly Mock<ILeaderboardRepository> _repo = new();
    private readonly LeaderboardController _sut;

    public LeaderboardControllerTests()
    {
        _sut = new LeaderboardController(_repo.Object, Mock.Of<ILogger<LeaderboardController>>());
    }

    [Fact]
    public async Task GetTopScores_ReturnsEntries()
    {
        var entries = new List<LeaderboardEntry>
        {
            new() { Rank = 1, PlayerName = "kiran", Score = 1000 }
        };
        _repo.Setup(r => r.GetTopScoresAsync("all", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetTopScores("all", 10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<List<LeaderboardEntry>>(ok.Value);
        Assert.Single(data);
    }

    [Fact]
    public async Task GetPlayerStats_NotFound_Returns404()
    {
        _repo.Setup(r => r.GetPlayerStatsAsync("nobody", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerStats?)null);

        var result = await _sut.GetPlayerStats("nobody", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPlayerStats_ReturnsStats()
    {
        var stats = new PlayerStats { PlayerName = "kiran", GamesPlayed = 5, GamesWon = 3 };
        _repo.Setup(r => r.GetPlayerStatsAsync("kiran", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetPlayerStats("kiran", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PlayerStats>(ok.Value);
        Assert.Equal("kiran", data.PlayerName);
    }
}
