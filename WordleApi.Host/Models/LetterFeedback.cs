namespace WordleApi.Host.Models;

public class LetterFeedback
{
    public char Letter { get; set; }
    public int Position { get; set; }
    public LetterResult Result { get; set; }
}
