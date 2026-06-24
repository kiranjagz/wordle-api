namespace WordleApi.Host.Models;

public class PlayerStats
{
    public string PlayerName { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public double WinRate { get; set; }
    public double AverageAttempts { get; set; }
    public double AverageTimeSeconds { get; set; }
    public int BestScore { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
}
