using WordleApi.Host.Models;

namespace WordleApi.Host.Data;

public interface ILeaderboardRepository
{
    Task<List<LeaderboardEntry>> GetTopScoresAsync(string period, int limit, CancellationToken ct = default);
    Task<PlayerStats?> GetPlayerStatsAsync(string playerName, CancellationToken ct = default);
    Task RefreshLeaderboardAsync(CancellationToken ct = default);
}
