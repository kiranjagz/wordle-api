using System.ComponentModel.DataAnnotations;

namespace WordleApi.Host.Models;

public class CreateGameRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string PlayerName { get; set; } = string.Empty;
}
