using Microsoft.AspNetCore.Mvc;
using Moq;
using WordleApi.Host.Controllers;
using WordleApi.Host.Models;
using WordleApi.Host.Services;
using Xunit;

namespace WordleApi.Tests.Controllers;

public class GamesControllerTests
{
    private readonly Mock<IGameService> _gameService = new();
    private readonly GamesController _sut;

    public GamesControllerTests()
    {
        _sut = new GamesController(_gameService.Object);
    }

    [Fact]
    public async Task CreateGame_Returns201WithGame()
    {
        var game = new Game
        {
            GameId = Guid.NewGuid(),
            PlayerName = "kiran",
            SecretWord = "crane",
            Status = GameStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            Guesses = []
        };
        _gameService.Setup(s => s.CreateGameAsync("kiran", It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await _sut.CreateGame(new CreateGameRequest { PlayerName = "kiran" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        var response = Assert.IsType<GameResponse>(created.Value);
        Assert.Equal("kiran", response.PlayerName);
        Assert.Null(response.SecretWord);
    }

    [Fact]
    public async Task GetGame_NotFound_Returns404()
    {
        _gameService.Setup(s => s.GetGameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var result = await _sut.GetGame(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetGame_FinishedGame_RevealsSecretWord()
    {
        var game = new Game
        {
            GameId = Guid.NewGuid(),
            PlayerName = "kiran",
            SecretWord = "crane",
            Status = GameStatus.Won,
            AttemptsUsed = 3,
            Score = 700,
            StartedAt = DateTime.UtcNow.AddMinutes(-1),
            CompletedAt = DateTime.UtcNow,
            Guesses = []
        };
        _gameService.Setup(s => s.GetGameAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await _sut.GetGame(game.GameId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GameResponse>(ok.Value);
        Assert.Equal("crane", response.SecretWord);
    }

    [Fact]
    public async Task SubmitGuess_GameNotFound_Returns404()
    {
        _gameService.Setup(s => s.SubmitGuessAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.SubmitGuess(Guid.NewGuid(), new SubmitGuessRequest { Word = "crane" }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SubmitGuess_GameFinished_Returns409()
    {
        _gameService.Setup(s => s.SubmitGuessAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Game is already finished"));

        var result = await _sut.SubmitGuess(Guid.NewGuid(), new SubmitGuessRequest { Word = "crane" }, CancellationToken.None);

        var conflict = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task SubmitGuess_InvalidWord_Returns400()
    {
        _gameService.Setup(s => s.SubmitGuessAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid word"));

        var result = await _sut.SubmitGuess(Guid.NewGuid(), new SubmitGuessRequest { Word = "zzzzz" }, CancellationToken.None);

        var bad = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_Returns204()
    {
        var gameId = Guid.NewGuid();
        _gameService.Setup(s => s.DeleteGameAsync(gameId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteGame(gameId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
