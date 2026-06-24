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

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var game = await _gameService.CreateGameAsync(request.PlayerName, ct);
        var response = MapToResponse(game);
        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, response);
    }

    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGame(Guid gameId, CancellationToken ct)
    {
        var game = await _gameService.GetGameAsync(gameId, ct);
        if (game is null)
            return NotFound();

        return Ok(MapToResponse(game));
    }

    [HttpPost("{gameId:guid}/guesses")]
    [ProducesResponseType(typeof(GuessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitGuess(Guid gameId, [FromBody] SubmitGuessRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _gameService.SubmitGuessAsync(gameId, request.Word, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Game already finished",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
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
        try
        {
            await _gameService.DeleteGameAsync(gameId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
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
