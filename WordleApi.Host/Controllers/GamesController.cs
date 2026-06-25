using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WordleApi.Host.Models;
using WordleApi.Host.Services;

namespace WordleApi.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating new game for player {PlayerName}", request.PlayerName);
        var game = await _gameService.CreateGameAsync(request.PlayerName, ct);
        _logger.LogInformation("Game {GameId} created for player {PlayerName}", game.GameId, game.PlayerName);
        var response = MapToResponse(game);
        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, response);
    }

    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGame(Guid gameId, CancellationToken ct)
    {
        _logger.LogInformation("Retrieving game {GameId}", gameId);
        var game = await _gameService.GetGameAsync(gameId, ct);
        if (game is null)
        {
            _logger.LogWarning("Game {GameId} not found", gameId);
            return NotFound();
        }

        return Ok(MapToResponse(game));
    }

    [HttpPost("{gameId:guid}/guesses")]
    [ProducesResponseType(typeof(GuessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitGuess(Guid gameId, [FromBody] SubmitGuessRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Submitting guess for game {GameId}, word {Word}", gameId, request.Word);
        try
        {
            var result = await _gameService.SubmitGuessAsync(gameId, request.Word, ct);
            _logger.LogInformation("Guess accepted for game {GameId}, attempt {AttemptNumber}, status {GameStatus}",
                gameId, result.AttemptNumber, result.GameStatus);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Game {GameId} not found for guess submission", gameId);
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Game {GameId} already finished: {Reason}", gameId, ex.Message);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Game already finished",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid word {Word} for game {GameId}: {Reason}", request.Word, gameId, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid word",
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame(Guid gameId, CancellationToken ct)
    {
        _logger.LogInformation("Deleting game {GameId}", gameId);
        try
        {
            await _gameService.DeleteGameAsync(gameId, ct);
            _logger.LogInformation("Game {GameId} deleted", gameId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Game {GameId} not found for deletion", gameId);
            return NotFound();
        }
    }

    private static GameResponse MapToResponse(Game game)
    {
        var response = new GameResponse
        {
            GameId = game.GameId,
            PlayerName = game.PlayerName,
            Status = game.Status,
            AttemptsUsed = game.AttemptsUsed,
            StartedAt = game.StartedAt,
            Score = game.Score,
            CompletedAt = game.CompletedAt,
            Guesses = game.Guesses.Select(g => new GuessHistoryItem
            {
                AttemptNumber = g.AttemptNumber,
                Word = g.Word,
                Letters = JsonSerializer.Deserialize<List<LetterFeedback>>(g.ResultJson) ?? [],
                GuessedAt = g.GuessedAt
            }).ToList()
        };

        if (game.Status != GameStatus.InProgress)
            response.SecretWord = game.SecretWord;

        return response;
    }
}
