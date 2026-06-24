using System.Data;

namespace WordleApi.Host.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken ct = default);
}
