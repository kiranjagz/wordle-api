using System.Text.Json;
using WordleApi.Host.Data;
using WordleApi.Host.Models;

namespace WordleApi.Host.Services;

public class GameService : IGameService
{
    private const int MaxAttempts = 6;

    private readonly IGameRepository _gameRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IWordService _wordService;

    public GameService(
        IGameRepository gameRepository,
        ILeaderboardRepository leaderboardRepository,
        IWordService wordService)
    {
        _gameRepository = gameRepository;
        _leaderboardRepository = leaderboardRepository;
        _wordService = wordService;
    }

    public async Task<Game> CreateGameAsync(string playerName, CancellationToken ct = default)
    {
        var game = new Game
        {
            GameId = Guid.NewGuid(),
            PlayerName = playerName.Trim(),
            SecretWord = _wordService.GetRandomWord(),
            Status = GameStatus.InProgress,
            AttemptsUsed = 0,
            StartedAt = DateTime.UtcNow
        };

        await _gameRepository.CreateAsync(game, ct);
        return game;
    }

    public async Task<Game?> GetGameAsync(Guid gameId, CancellationToken ct = default)
    {
        return await _gameRepository.GetByIdAsync(gameId, ct);
    }

    public async Task<GuessResult> SubmitGuessAsync(Guid gameId, string word, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new KeyNotFoundException($"Game {gameId} not found");

        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is already finished");

        var normalizedWord = word.Trim().ToLowerInvariant();

        if (!_wordService.IsValidWord(normalizedWord))
            throw new ArgumentException($"'{word}' is not a valid 5-letter word");

        var letters = _wordService.Evaluate(normalizedWord, game.SecretWord);
        var isCorrect = letters.All(l => l.Result == LetterResult.Correct);

        game.AttemptsUsed++;

        var guess = new Guess
        {
            GuessId = Guid.NewGuid(),
            GameId = gameId,
            AttemptNumber = game.AttemptsUsed,
            Word = normalizedWord,
            ResultJson = JsonSerializer.Serialize(letters),
            GuessedAt = DateTime.UtcNow
        };

        await _gameRepository.AddGuessAsync(guess, ct);

        if (isCorrect)
        {
            game.Status = GameStatus.Won;
            game.CompletedAt = DateTime.UtcNow;
            game.Score = _wordService.CalculateScore(game.AttemptsUsed, game.CompletedAt.Value - game.StartedAt);
        }
        else if (game.AttemptsUsed >= MaxAttempts)
        {
            game.Status = GameStatus.Lost;
            game.CompletedAt = DateTime.UtcNow;
        }

        await _gameRepository.UpdateAsync(game, ct);

        if (game.Status == GameStatus.Won)
            await _leaderboardRepository.RefreshLeaderboardAsync(ct);

        var result = new GuessResult
        {
            AttemptNumber = game.AttemptsUsed,
            Word = normalizedWord,
            Letters = letters.ToList(),
            GameStatus = game.Status,
            AttemptsRemaining = MaxAttempts - game.AttemptsUsed
        };

        if (game.Status == GameStatus.Won)
        {
            result.Score = game.Score;
            result.CompletedAt = game.CompletedAt;
            result.SecretWord = game.SecretWord;
        }
        else if (game.Status == GameStatus.Lost)
        {
            result.SecretWord = game.SecretWord;
        }

        return result;
    }

    public async Task DeleteGameAsync(Guid gameId, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new KeyNotFoundException($"Game {gameId} not found");

        await _gameRepository.DeleteAsync(gameId, ct);
    }
}
