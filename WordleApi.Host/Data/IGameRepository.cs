using WordleApi.Host.Models;

namespace WordleApi.Host.Data;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid gameId, CancellationToken ct = default);
    Task CreateAsync(Game game, CancellationToken ct = default);
    Task UpdateAsync(Game game, CancellationToken ct = default);
    Task DeleteAsync(Guid gameId, CancellationToken ct = default);
    Task AddGuessAsync(Guess guess, CancellationToken ct = default);
    Task<List<Guess>> GetGuessesAsync(Guid gameId, CancellationToken ct = default);
}
