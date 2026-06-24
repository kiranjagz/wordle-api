namespace WordleApi.Host.Models;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public int TimeTakenSeconds { get; set; }
    public int Score { get; set; }
    public DateTime CompletedAt { get; set; }
    public string Word { get; set; } = string.Empty;
}
