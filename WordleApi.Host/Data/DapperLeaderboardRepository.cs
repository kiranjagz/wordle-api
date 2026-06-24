using Dapper;
using WordleApi.Host.Models;

namespace WordleApi.Host.Data;

public class DapperLeaderboardRepository : ILeaderboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperLeaderboardRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<LeaderboardEntry>> GetTopScoresAsync(string period, int limit, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        var cutoff = period.ToLowerInvariant() switch
        {
            "day" => DateTime.UtcNow.AddDays(-1),
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddMonths(-1),
            _ => DateTime.MinValue
        };

        var entries = await connection.QueryAsync<LeaderboardEntry>("""
            SELECT
                ROW_NUMBER() OVER (ORDER BY score DESC, completed_at ASC) AS Rank,
                player_name AS PlayerName,
                attempts_used AS Attempts,
                time_taken_seconds AS TimeTakenSeconds,
                score AS Score,
                completed_at AS CompletedAt,
                secret_word AS Word
            FROM leaderboard_rankings
            WHERE completed_at >= @Cutoff
            ORDER BY score DESC, completed_at ASC
            LIMIT @Limit
            """, new { Cutoff = cutoff, Limit = limit });

        return entries.ToList();
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(string playerName, CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        var stats = await connection.QuerySingleOrDefaultAsync<PlayerStats>("""
            SELECT
                player_name AS PlayerName,
                COUNT(*) AS GamesPlayed,
                COUNT(*) FILTER (WHERE status = 1) AS GamesWon,
                CASE WHEN COUNT(*) > 0
                    THEN ROUND(COUNT(*) FILTER (WHERE status = 1)::NUMERIC / COUNT(*), 2)
                    ELSE 0
                END AS WinRate,
                COALESCE(AVG(attempts_used) FILTER (WHERE status = 1), 0) AS AverageAttempts,
                COALESCE(AVG(EXTRACT(EPOCH FROM (completed_at - started_at))) FILTER (WHERE status = 1), 0) AS AverageTimeSeconds,
                COALESCE(MAX(score) FILTER (WHERE status = 1), 0) AS BestScore
            FROM games
            WHERE player_name = @PlayerName
            GROUP BY player_name
            """, new { PlayerName = playerName });

        if (stats is null)
            return null;

        var streaks = await CalculateStreaksAsync(connection, playerName);
        stats.CurrentStreak = streaks.Current;
        stats.MaxStreak = streaks.Max;

        return stats;
    }

    public async Task RefreshLeaderboardAsync(CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW CONCURRENTLY leaderboard_rankings");
    }

    private static async Task<(int Current, int Max)> CalculateStreaksAsync(
        System.Data.IDbConnection connection, string playerName)
    {
        var games = await connection.QueryAsync<int>("""
            SELECT status
            FROM games
            WHERE player_name = @PlayerName AND status IN (1, 2)
            ORDER BY completed_at DESC
            """, new { PlayerName = playerName });

        var currentStreak = 0;
        var maxStreak = 0;
        var streak = 0;
        var countingCurrent = true;

        foreach (var status in games)
        {
            if (status == 1) // Won
            {
                streak++;
                if (countingCurrent)
                    currentStreak = streak;
            }
            else
            {
                maxStreak = Math.Max(maxStreak, streak);
                streak = 0;
                countingCurrent = false;
            }
        }

        maxStreak = Math.Max(maxStreak, streak);
        return (currentStreak, maxStreak);
    }
}
