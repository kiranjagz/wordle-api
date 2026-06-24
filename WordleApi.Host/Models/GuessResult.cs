using System.Text.Json.Serialization;

namespace WordleApi.Host.Models;

public class GuessResult
{
    public int AttemptNumber { get; set; }
    public string Word { get; set; } = string.Empty;
    public List<LetterFeedback> Letters { get; set; } = [];
    public GameStatus GameStatus { get; set; }
    public int AttemptsRemaining { get; set; }
    public int? Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SecretWord { get; set; }
}
