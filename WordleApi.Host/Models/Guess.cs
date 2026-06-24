namespace WordleApi.Host.Models;

public class Guess
{
    public Guid GuessId { get; set; }
    public Guid GameId { get; set; }
    public int AttemptNumber { get; set; }
    public string Word { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public DateTime GuessedAt { get; set; }
}
