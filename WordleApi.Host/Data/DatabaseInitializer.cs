using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;

namespace WordleApi.Host.Data;

public partial class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version     INTEGER PRIMARY KEY,
                name        TEXT NOT NULL,
                applied_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
            """);

        var applied = (await connection.QueryAsync<int>(
            "SELECT version FROM schema_migrations ORDER BY version"))
            .ToHashSet();

        var migrations = GetMigrations();

        foreach (var (version, name, sql) in migrations.OrderBy(m => m.Version))
        {
            if (applied.Contains(version))
                continue;

            _logger.LogInformation("Applying migration {Version}: {Name}", version, name);

            using var transaction = connection.BeginTransaction();
            await connection.ExecuteAsync(sql, transaction: transaction);
            await connection.ExecuteAsync(
                "INSERT INTO schema_migrations (version, name) VALUES (@Version, @Name)",
                new { Version = version, Name = name },
                transaction: transaction);
            transaction.Commit();

            _logger.LogInformation("Migration {Version} applied successfully", version);
        }
    }

    private static List<(int Version, string Name, string Sql)> GetMigrations()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = "WordleApi.Host.Data.Migrations.";
        var results = new List<(int Version, string Name, string Sql)>();

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix) && n.EndsWith(".sql")))
        {
            var fileName = resourceName[prefix.Length..];
            var match = MigrationPattern().Match(fileName);

            if (!match.Success)
                continue;

            var version = int.Parse(match.Groups[1].Value);
            var name = match.Groups[2].Value.Replace('_', ' ');

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var sql = reader.ReadToEnd();

            results.Add((version, name, sql));
        }

        return results;
    }

    [GeneratedRegex(@"^(\d+)_(.+)\.sql$")]
    private static partial Regex MigrationPattern();
}
