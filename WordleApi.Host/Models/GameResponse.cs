using System.Text.Json.Serialization;

namespace WordleApi.Host.Models;

public class GameResponse
{
    public Guid GameId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public int MaxAttempts { get; set; } = 6;
    public int AttemptsUsed { get; set; }
    public List<GuessHistoryItem> Guesses { get; set; } = [];
    public DateTime StartedAt { get; set; }
    public int? Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SecretWord { get; set; }
}

public class GuessHistoryItem
{
    public int AttemptNumber { get; set; }
    public string Word { get; set; } = string.Empty;
    public List<LetterFeedback> Letters { get; set; } = [];
    public DateTime GuessedAt { get; set; }
}
