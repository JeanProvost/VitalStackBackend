namespace Backend.Core.Entities.Users.DTOs;

public class ThirdPartyAuthorizationUrlResponseDto
{
    public ThirdPartyAuthProvider Provider { get; set; }
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = [];
}
