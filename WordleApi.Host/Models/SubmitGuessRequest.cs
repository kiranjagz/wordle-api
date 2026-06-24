using System.ComponentModel.DataAnnotations;

namespace WordleApi.Host.Models;

public class SubmitGuessRequest
{
    [Required]
    [StringLength(5, MinimumLength = 5)]
    [RegularExpression(@"^[a-zA-Z]+$")]
    public string Word { get; set; } = string.Empty;
}
