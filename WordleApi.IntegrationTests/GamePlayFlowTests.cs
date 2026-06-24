using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using WordleApi.Host.Data;
using WordleApi.Host.Models;
using Xunit;

namespace WordleApi.IntegrationTests;

public class GamePlayFlowTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("wordleapi_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.Single(d => d.ServiceType == typeof(IDbConnectionFactory));
                    services.Remove(descriptor);
                    services.AddSingleton<IDbConnectionFactory>(
                        new TestConnectionFactory(_postgres.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FullGameFlow_CreatePlayAndCheckLeaderboard()
    {
        // Create a game
        var createResponse = await _client.PostAsJsonAsync("/api/games", new { playerName = "integration-test" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var game = await createResponse.Content.ReadFromJsonAsync<GameResponse>();
        Assert.NotNull(game);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.SecretWord);

        // Get game state
        var getResponse = await _client.GetFromJsonAsync<GameResponse>($"/api/games/{game.GameId}");
        Assert.NotNull(getResponse);
        Assert.Equal("integration-test", getResponse.PlayerName);

        // Submit some guesses (we don't know the secret word, so just test the flow)
        var guessResponse = await _client.PostAsJsonAsync(
            $"/api/games/{game.GameId}/guesses",
            new { word = "crane" });
        Assert.Equal(HttpStatusCode.OK, guessResponse.StatusCode);

        var guessResult = await guessResponse.Content.ReadFromJsonAsync<GuessResult>();
        Assert.NotNull(guessResult);
        Assert.Equal(1, guessResult.AttemptNumber);
        Assert.Equal(5, guessResult.Letters.Count);

        // Check leaderboard endpoint works
        var leaderboard = await _client.GetAsync("/api/leaderboard");
        Assert.Equal(HttpStatusCode.OK, leaderboard.StatusCode);
    }

    [Fact]
    public async Task SubmitGuess_InvalidWord_Returns400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/games", new { playerName = "test" });
        var game = await createResponse.Content.ReadFromJsonAsync<GameResponse>();

        var guessResponse = await _client.PostAsJsonAsync(
            $"/api/games/{game!.GameId}/guesses",
            new { word = "zzzzz" });

        Assert.Equal(HttpStatusCode.BadRequest, guessResponse.StatusCode);
    }

    [Fact]
    public async Task GetGame_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_RemovesGame()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/games", new { playerName = "delete-test" });
        var game = await createResponse.Content.ReadFromJsonAsync<GameResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/games/{game!.GameId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/games/{game.GameId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private class TestConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public TestConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<System.Data.IDbConnection> CreateConnectionAsync(CancellationToken ct = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            return connection;
        }
    }
}
