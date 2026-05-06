using Amazon.CognitoIdentityProvider.Model;
using Backend.Core.Configuration;
using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IRepository;
using Backend.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Core.Services;

public class UserService(
    ICognitoAuthService _cognitoAuthService,
    IUserRepository _userRepository,
    IOptions<AwsCognitoSettings> _cognitoSettings,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var cognitoSub = await _cognitoAuthService.SignUpAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        logger.LogInformation("Cognito registration successful, saving user to database");

        var user = new User
        {
            IdentityId = cognitoSub,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = "cognito",
        };

        var createdUser = await _userRepository.Create(user);

        logger.LogInformation("User saved to database with Id {UserId}", createdUser.Id);

        return createdUser;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting login for {Email}", request.Email);

        var signUpRequest = new SignUpRequest
        {
            ClientId = _cognitoSettings.Value.ClientId,
            Username = request.Email,
            Password = request.Password,
            UserAttributes =
            [
                new AttributeType { Name = "email", Value = request.Email },
            ]
        };

        logger.LogInformation("Sending login request to Cognito for {Email}", request.Email);

        //var response = await _

    }
}