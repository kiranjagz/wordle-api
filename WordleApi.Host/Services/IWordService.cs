using WordleApi.Host.Models;

namespace WordleApi.Host.Services;

public interface IWordService
{
    string GetRandomWord();
    bool IsValidWord(string word);
    LetterFeedback[] Evaluate(string guess, string secret);
    int CalculateScore(int attempts, TimeSpan duration);
}
