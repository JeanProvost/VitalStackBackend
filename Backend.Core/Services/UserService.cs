using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Backend.Core.Configuration;
using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IRepository;
using Backend.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata.Ecma335;

namespace Backend.Core.Services;

public class UserService(
    IAmazonCognitoIdentityProvider _cognitoAuthService,
    IUserRepository _userRepository,
    IOptions<AwsCognitoSettings> _cognitoSettings,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (existingUser)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }

        logger.LogInformation($"Initiating regisration for {request.Email}");

        var signUpRequest = new SignUpRequest
        {
            ClientId = _cognitoSettings.Value.ClientId,
            Username = request.Email,
            Password = request.Password,
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = request.Email },
                new() { Name = "given_name", Value = request.FirstName },
                new() { Name = "family_name", Value = request.LastName }
            }
        };

        var response = await _cognitoAuthService.SignUpAsync(signUpRequest, cancellationToken);

        var user = new User
        {
            IdentityId = response.UserSub,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = "cognito",
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var createdUser = await _userRepository.Create(user);
            logger.LogInformation($"User {user.Id} registered and saved successfully.");

            return createdUser;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to save user to database after Cognito success for email {request.Email}.");
            throw;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting login for {Email}", request.Email);

        try
        {
            var authRequest = new InitiateAuthRequest
            {
                ClientId = _cognitoSettings.Value.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", request.Email },
                    { "PASSWORD", request.Password }
                }
            };

            var response = await _cognitoAuthService.InitiateAuthAsync(authRequest, cancellationToken);
            var authResponse = response.AuthenticationResult 
                ?? throw new UnauthorizedAccessException("Authentication failed");

            return new LoginResponseDto
            {
                AccessToken = authResponse.AccessToken ?? string.Empty,
                IdToken = authResponse.IdToken ?? string.Empty,
                RefreshToken = authResponse.RefreshToken ?? string.Empty,
                ExpiresIn = (int)authResponse.ExpiresIn,
                TokenType = authResponse.TokenType ?? "Bearer"
            };
        }
        catch (NotAuthorizedException)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        catch (UserNotConfirmedException)
        {
            throw new InvalidOperationException("User account is not confirmed");
        }
    }
}