using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Entities.Users.DTOs;

public class ThirdPartyLoginDto
{
    [Required]
    public string AuthorizationCode { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string CodeVerifier { get; set; } = string.Empty;
}
