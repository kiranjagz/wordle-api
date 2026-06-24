using WordleApi.Host.Models;

namespace WordleApi.Host.Services;

public interface IGameService
{
    Task<Game> CreateGameAsync(string playerName, CancellationToken ct = default);
    Task<Game?> GetGameAsync(Guid gameId, CancellationToken ct = default);
    Task<GuessResult> SubmitGuessAsync(Guid gameId, string word, CancellationToken ct = default);
    Task DeleteGameAsync(Guid gameId, CancellationToken ct = default);
}
