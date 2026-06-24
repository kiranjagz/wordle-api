using System.Reflection;
using WordleApi.Host.Models;

namespace WordleApi.Host.Services;

public class WordService : IWordService
{
    private readonly List<string> _words;
    private readonly HashSet<string> _validWords;

    public WordService()
    {
        _words = LoadWords();
        _validWords = new HashSet<string>(_words, StringComparer.OrdinalIgnoreCase);
    }

    public string GetRandomWord()
    {
        return _words[Random.Shared.Next(_words.Count)];
    }

    public bool IsValidWord(string word)
    {
        return word.Length == 5 && _validWords.Contains(word);
    }

    public LetterFeedback[] Evaluate(string guess, string secret)
    {
        var results = new LetterFeedback[5];
        var available = new Dictionary<char, int>();

        foreach (var c in secret)
            available[c] = available.GetValueOrDefault(c) + 1;

        // Pass 1: exact matches
        for (var i = 0; i < 5; i++)
        {
            results[i] = new LetterFeedback { Letter = guess[i], Position = i };

            if (guess[i] == secret[i])
            {
                results[i].Result = LetterResult.Correct;
                available[guess[i]]--;
            }
        }

        // Pass 2: present / absent
        for (var i = 0; i < 5; i++)
        {
            if (results[i].Result == LetterResult.Correct)
                continue;

            if (available.GetValueOrDefault(guess[i]) > 0)
            {
                results[i].Result = LetterResult.Present;
                available[guess[i]]--;
            }
            else
            {
                results[i].Result = LetterResult.Absent;
            }
        }

        return results;
    }

    public int CalculateScore(int attempts, TimeSpan duration)
    {
        var timePenalty = (int)(duration.TotalSeconds / 10);
        return Math.Max(0, 1000 - (attempts - 1) * 150 - timePenalty);
    }

    private static List<string> LoadWords()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith("words.txt"));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);

        var words = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim().ToLowerInvariant();
            if (trimmed.Length == 5 && trimmed.All(char.IsLetter))
                words.Add(trimmed);
        }

        return words;
    }
}
