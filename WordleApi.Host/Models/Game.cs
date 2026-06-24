namespace WordleApi.Host.Models;

public class Game
{
    public Guid GameId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string SecretWord { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public int AttemptsUsed { get; set; }
    public int? Score { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<Guess> Guesses { get; set; } = [];
}
