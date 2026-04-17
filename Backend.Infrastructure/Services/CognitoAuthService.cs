using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Backend.Core.Configuration;
using Backend.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

public class CognitoAuthService(
    IAmazonCognitoIdentityProvider cognitoClient,
    IOptions<AwsCognitoSettings> cognitoSettings,
    ILogger<CognitoAuthService> logger) : ICognitoAuthService
{
    private readonly AwsCognitoSettings _settings = cognitoSettings.Value;

    public async Task<string> SignUpAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _settings.ClientId,
            Username = email,
            Password = password,
            UserAttributes =
            [
                new AttributeType { Name = "email", Value = email },
                new AttributeType { Name = "given_name", Value = firstName },
                new AttributeType { Name = "family_name", Value = lastName }
            ]
        };

        logger.LogInformation("Registering user in Cognito");

        var response = await cognitoClient.SignUpAsync(signUpRequest, cancellationToken);

        logger.LogInformation("User registered in Cognito successfully");

        return response.UserSub;
    }
}
