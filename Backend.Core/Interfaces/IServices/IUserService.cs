using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;

namespace Backend.Core.Interfaces.IServices;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken);
    ThirdPartyAuthorizationUrlResponseDto GetThirdPartyAuthorizationUrl(
        ThirdPartyAuthProvider provider,
        string? redirectUri = null,
        string? state = null,
        string? codeChallenge = null);
    Task<LoginResponseDto> ThirdPartyLoginAsync(
        ThirdPartyAuthProvider provider,
        ThirdPartyLoginDto request,
        CancellationToken cancellationToken);
}
