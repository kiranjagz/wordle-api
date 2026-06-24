using Moq;
using WordleApi.Host.Data;
using WordleApi.Host.Models;
using WordleApi.Host.Services;
using Xunit;

namespace WordleApi.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _gameRepo = new();
    private readonly Mock<ILeaderboardRepository> _leaderboardRepo = new();
    private readonly Mock<IWordService> _wordService = new();
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _wordService.Setup(w => w.GetRandomWord()).Returns("crane");
        _wordService.Setup(w => w.IsValidWord(It.IsAny<string>())).Returns(true);
        _wordService.Setup(w => w.Evaluate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string guess, string secret) =>
                guess.Select((c, i) => new LetterFeedback
                {
                    Letter = c,
                    Position = i,
                    Result = c == secret[i] ? LetterResult.Correct : LetterResult.Absent
                }).ToArray());
        _wordService.Setup(w => w.CalculateScore(It.IsAny<int>(), It.IsAny<TimeSpan>())).Returns(1000);

        _sut = new GameService(_gameRepo.Object, _leaderboardRepo.Object, _wordService.Object);
    }

    [Fact]
    public async Task CreateGame_ReturnsNewInProgressGame()
    {
        var game = await _sut.CreateGameAsync("kiran");

        Assert.Equal("kiran", game.PlayerName);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(0, game.AttemptsUsed);
        Assert.Equal("crane", game.SecretWord);
        _gameRepo.Verify(r => r.CreateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitGuess_IncrementsAttempts()
    {
        var game = CreateTestGame();
        _gameRepo.Setup(r => r.GetByIdAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await _sut.SubmitGuessAsync(game.GameId, "wrong");

        Assert.Equal(1, result.AttemptNumber);
        Assert.Equal(5, result.AttemptsRemaining);
        Assert.Equal(GameStatus.InProgress, result.GameStatus);
    }

    [Fact]
    public async Task SubmitGuess_CorrectWord_SetsWonAndScore()
    {
        var game = CreateTestGame();
        _gameRepo.Setup(r => r.GetByIdAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await _sut.SubmitGuessAsync(game.GameId, "crane");

        Assert.Equal(GameStatus.Won, result.GameStatus);
        Assert.NotNull(result.Score);
        Assert.Equal("crane", result.SecretWord);
        _leaderboardRepo.Verify(r => r.RefreshLeaderboardAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitGuess_SixthWrongGuess_SetsLost()
    {
        var game = CreateTestGame();
        game.AttemptsUsed = 5;
        _gameRepo.Setup(r => r.GetByIdAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await _sut.SubmitGuessAsync(game.GameId, "wrong");

        Assert.Equal(GameStatus.Lost, result.GameStatus);
        Assert.Equal(0, result.AttemptsRemaining);
        Assert.Equal("crane", result.SecretWord);
    }

    [Fact]
    public async Task SubmitGuess_OnFinishedGame_ThrowsInvalidOperation()
    {
        var game = CreateTestGame();
        game.Status = GameStatus.Won;
        _gameRepo.Setup(r => r.GetByIdAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitGuessAsync(game.GameId, "crane"));
    }

    [Fact]
    public async Task SubmitGuess_InvalidWord_ThrowsArgument()
    {
        var game = CreateTestGame();
        _gameRepo.Setup(r => r.GetByIdAsync(game.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        _wordService.Setup(w => w.IsValidWord("zzzzz")).Returns(false);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SubmitGuessAsync(game.GameId, "zzzzz"));
    }

    [Fact]
    public async Task SubmitGuess_GameNotFound_ThrowsKeyNotFound()
    {
        _gameRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.SubmitGuessAsync(Guid.NewGuid(), "crane"));
    }

    private static Game CreateTestGame() => new()
    {
        GameId = Guid.NewGuid(),
        PlayerName = "tester",
        SecretWord = "crane",
        Status = GameStatus.InProgress,
        AttemptsUsed = 0,
        StartedAt = DateTime.UtcNow,
        Guesses = []
    };
}
