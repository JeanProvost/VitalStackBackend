using System.ComponentModel.DataAnnotations;

namespace Backend.Core.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
