using Dapper;
using WordleApi.Host.Models;

namespace WordleApi.Host.Data;

public class DapperGameRepository : IGameRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperGameRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Game?> GetByIdAsync(Guid gameId, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        var game = await connection.QuerySingleOrDefaultAsync<Game>("""
            SELECT game_id AS GameId, player_name AS PlayerName, secret_word AS SecretWord,
                   status AS Status, attempts_used AS AttemptsUsed, score AS Score,
                   started_at AS StartedAt, completed_at AS CompletedAt
            FROM games
            WHERE game_id = @GameId
            """, new { GameId = gameId });

        if (game is not null)
            game.Guesses = await GetGuessesAsync(gameId, ct);

        return game;
    }

    public async Task CreateAsync(Game game, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        await connection.ExecuteAsync("""
            INSERT INTO games (game_id, player_name, secret_word, status, attempts_used, started_at)
            VALUES (@GameId, @PlayerName, @SecretWord, @Status, @AttemptsUsed, @StartedAt)
            """, game);
    }

    public async Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        await connection.ExecuteAsync("""
            UPDATE games
            SET status = @Status, attempts_used = @AttemptsUsed, score = @Score, completed_at = @CompletedAt
            WHERE game_id = @GameId
            """, game);
    }

    public async Task DeleteAsync(Guid gameId, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        await connection.ExecuteAsync("DELETE FROM games WHERE game_id = @GameId", new { GameId = gameId });
    }

    public async Task AddGuessAsync(Guess guess, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        await connection.ExecuteAsync("""
            INSERT INTO guesses (guess_id, game_id, attempt_number, word, result_json, guessed_at)
            VALUES (@GuessId, @GameId, @AttemptNumber, @Word, @ResultJson::jsonb, @GuessedAt)
            """, guess);
    }

    public async Task<List<Guess>> GetGuessesAsync(Guid gameId, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        var guesses = await connection.QueryAsync<Guess>("""
            SELECT guess_id AS GuessId, game_id AS GameId, attempt_number AS AttemptNumber,
                   word AS Word, result_json AS ResultJson, guessed_at AS GuessedAt
            FROM guesses
            WHERE game_id = @GameId
            ORDER BY attempt_number
            """, new { GameId = gameId });

        return guesses.ToList();
    }
}
