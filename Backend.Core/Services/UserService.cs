using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IRepository;
using Backend.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace Backend.Core.Services;

public class UserService(
    ICognitoAuthService cognitoAuthService,
    IUserRepository userRepository,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var cognitoSub = await cognitoAuthService.SignUpAsync(
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

        var createdUser = await userRepository.Create(user);

        logger.LogInformation("User saved to database with Id {UserId}", createdUser.Id);

        return createdUser;
    }
}
