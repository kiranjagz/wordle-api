using WordleApi.Host.Models;
using WordleApi.Host.Services;
using Xunit;

namespace WordleApi.Tests.Services;

public class WordServiceTests
{
    private readonly WordService _sut = new();

    [Fact]
    public void GetRandomWord_ReturnsAFiveLetterWord()
    {
        var word = _sut.GetRandomWord();
        Assert.Equal(5, word.Length);
        Assert.True(word.All(char.IsLetter));
    }

    [Fact]
    public void GetRandomWord_ReturnsDifferentWords()
    {
        var words = Enumerable.Range(0, 20).Select(_ => _sut.GetRandomWord()).ToHashSet();
        Assert.True(words.Count > 1);
    }

    [Theory]
    [InlineData("crane", true)]
    [InlineData("about", true)]
    [InlineData("CRANE", true)]
    [InlineData("zzzzz", false)]
    [InlineData("hi", false)]
    [InlineData("toolong", false)]
    [InlineData("", false)]
    public void IsValidWord_ValidatesCorrectly(string word, bool expected)
    {
        Assert.Equal(expected, _sut.IsValidWord(word));
    }

    [Fact]
    public void Evaluate_AllCorrect()
    {
        var result = _sut.Evaluate("crane", "crane");
        Assert.All(result, l => Assert.Equal(LetterResult.Correct, l.Result));
    }

    [Fact]
    public void Evaluate_AllAbsent()
    {
        var result = _sut.Evaluate("dusty", "crane");
        Assert.All(result, l => Assert.Equal(LetterResult.Absent, l.Result));
    }

    [Fact]
    public void Evaluate_MixedResults()
    {
        // Secret: crane (c-r-a-n-e), Guess: chart (c-h-a-r-t)
        // c=Correct(pos0), h=Absent, a=Correct(pos2), r=Present, t=Absent
        var result = _sut.Evaluate("chart", "crane");

        Assert.Equal(LetterResult.Correct, result[0].Result); // c matches c
        Assert.Equal(LetterResult.Absent, result[1].Result);  // h not in crane
        Assert.Equal(LetterResult.Correct, result[2].Result);  // a matches a at pos 2
        Assert.Equal(LetterResult.Present, result[3].Result);  // r is in crane but wrong position
        Assert.Equal(LetterResult.Absent, result[4].Result);  // t not in crane
    }

    [Fact]
    public void Evaluate_DuplicateLetters_ExactMatchTakesPriority()
    {
        // Secret: creep (c-r-e-e-p), Guess: speed (s-p-e-e-d)
        // s=Absent, p=Present, e=Correct(pos2), e=Correct(pos3), d=Absent
        var result = _sut.Evaluate("speed", "creep");

        Assert.Equal(LetterResult.Absent, result[0].Result);   // s
        Assert.Equal(LetterResult.Present, result[1].Result);  // p (in creep but wrong pos)
        Assert.Equal(LetterResult.Correct, result[2].Result);  // e matches e at pos 2
        Assert.Equal(LetterResult.Correct, result[3].Result);  // e matches e at pos 3
        Assert.Equal(LetterResult.Absent, result[4].Result);   // d
    }

    [Fact]
    public void Evaluate_DuplicateLetters_LimitedAvailability()
    {
        // Secret: apple, Guess: puppy
        // p=Present, u=Absent, p=Correct(pos 2), p=Absent(no more p's), y=Absent
        var result = _sut.Evaluate("puppy", "apple");

        Assert.Equal(LetterResult.Present, result[0].Result);  // p (one p available)
        Assert.Equal(LetterResult.Absent, result[1].Result);   // u
        Assert.Equal(LetterResult.Correct, result[2].Result);  // p (exact match)
        Assert.Equal(LetterResult.Absent, result[3].Result);   // p (no more p's)
        Assert.Equal(LetterResult.Absent, result[4].Result);   // y
    }

    [Fact]
    public void Evaluate_SetsCorrectPositions()
    {
        var result = _sut.Evaluate("crane", "crane");
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(i, result[i].Position);
            Assert.Equal("crane"[i], result[i].Letter);
        }
    }

    [Theory]
    [InlineData(1, 0, 1000)]
    [InlineData(2, 0, 850)]
    [InlineData(3, 0, 700)]
    [InlineData(6, 0, 250)]
    [InlineData(1, 100, 990)]
    [InlineData(6, 300, 220)]
    [InlineData(6, 9999, 0)]
    public void CalculateScore_ReturnsExpectedValues(int attempts, int seconds, int expectedScore)
    {
        var score = _sut.CalculateScore(attempts, TimeSpan.FromSeconds(seconds));
        Assert.Equal(expectedScore, score);
    }
}
