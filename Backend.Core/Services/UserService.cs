using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Backend.Core.Configuration;
using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IRepository;
using Backend.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Core.Services;

public class UserService(
    IAmazonCognitoIdentityProvider _cognitoAuthService,
    IUserRepository _userRepository,
    IOptions<AwsCognitoSettings> _cognitoSettings,
    HttpClient _httpClient,
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
                ExpiresIn = (int)authResponse.ExpiresIn.GetValueOrDefault(),
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

    public ThirdPartyAuthorizationUrlResponseDto GetThirdPartyAuthorizationUrl(
        ThirdPartyAuthProvider provider,
        string? redirectUri = null,
        string? state = null,
        string? codeChallenge = null)
    {
        var settings = _cognitoSettings.Value;
        var resolvedRedirectUri = ResolveRedirectUri(settings, redirectUri);
        var hostedUiDomain = ResolveHostedUiDomain(settings);
        var scopes = ResolveScopes(settings);

        var queryParameters = new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["identity_provider"] = GetCognitoIdentityProvider(provider),
            ["redirect_uri"] = resolvedRedirectUri,
            ["response_type"] = "code",
            ["scope"] = string.Join(' ', scopes)
        };

        if (!string.IsNullOrWhiteSpace(state))
        {
            queryParameters["state"] = state;
        }

        if (!string.IsNullOrWhiteSpace(codeChallenge))
        {
            queryParameters["code_challenge"] = codeChallenge;
            queryParameters["code_challenge_method"] = "S256";
        }

        return new ThirdPartyAuthorizationUrlResponseDto
        {
            Provider = provider,
            AuthorizationUrl = $"{hostedUiDomain}/oauth2/authorize?{BuildQueryString(queryParameters)}",
            RedirectUri = resolvedRedirectUri,
            Scopes = scopes
        };
    }

    public async Task<LoginResponseDto> ThirdPartyLoginAsync(
        ThirdPartyAuthProvider provider,
        ThirdPartyLoginDto request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AuthorizationCode);

        var settings = _cognitoSettings.Value;
        var resolvedRedirectUri = ResolveRedirectUri(settings, request.RedirectUri);
        var hostedUiDomain = ResolveHostedUiDomain(settings);

        var formValues = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = settings.ClientId,
            ["code"] = request.AuthorizationCode,
            ["redirect_uri"] = resolvedRedirectUri
        };

        if (!string.IsNullOrWhiteSpace(request.CodeVerifier))
        {
            formValues["code_verifier"] = request.CodeVerifier;
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{hostedUiDomain}/oauth2/token")
        {
            Content = new FormUrlEncodedContent(formValues)
        };

        if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{settings.ClientId}:{settings.ClientSecret}"));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Third-party login failed for {Provider}. Cognito returned {StatusCode}: {Response}",
                provider,
                response.StatusCode,
                responseContent);

            throw new UnauthorizedAccessException("Third-party authentication failed");
        }

        var tokenResponse = JsonSerializer.Deserialize<CognitoTokenResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new UnauthorizedAccessException("Third-party authentication returned an empty response");

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken) ||
            string.IsNullOrWhiteSpace(tokenResponse.IdToken))
        {
            throw new UnauthorizedAccessException("Third-party authentication did not return the expected tokens");
        }

        await EnsureThirdPartyUserExistsAsync(provider, tokenResponse.IdToken, cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = tokenResponse.AccessToken,
            IdToken = tokenResponse.IdToken,
            RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
            ExpiresIn = tokenResponse.ExpiresIn,
            TokenType = tokenResponse.TokenType ?? "Bearer"
        };
    }

    private async Task EnsureThirdPartyUserExistsAsync(
        ThirdPartyAuthProvider provider,
        string idToken,
        CancellationToken cancellationToken)
    {
        var claims = ReadJwtPayloadClaims(idToken);

        if (!claims.TryGetValue("email", out var email) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation("Skipping local user creation because the ID token did not include an email claim.");
            return;
        }

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        if (!claims.TryGetValue("sub", out var subject) || string.IsNullOrWhiteSpace(subject))
        {
            logger.LogInformation("Skipping local user creation for {Email} because the ID token did not include a subject claim.", email);
            return;
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            IdentityId = subject,
            Email = email,
            FirstName = GetClaimValue(claims, "given_name"),
            LastName = GetClaimValue(claims, "family_name"),
            AuthProvider = provider.ToString().ToLowerInvariant(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _userRepository.Create(user);
    }

    private static string ResolveHostedUiDomain(AwsCognitoSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.HostedUiDomain))
        {
            throw new InvalidOperationException("AwsCognito:HostedUiDomain must be configured before third-party authentication can be used.");
        }

        var hostedUiDomain = settings.HostedUiDomain.Trim().TrimEnd('/');

        return hostedUiDomain.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            hostedUiDomain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? hostedUiDomain
            : $"https://{hostedUiDomain}";
    }

    private static string ResolveRedirectUri(AwsCognitoSettings settings, string? redirectUri)
    {
        var resolvedRedirectUri = string.IsNullOrWhiteSpace(redirectUri)
            ? settings.DefaultRedirectUri
            : redirectUri;

        if (string.IsNullOrWhiteSpace(resolvedRedirectUri))
        {
            throw new InvalidOperationException("Provide a redirectUri or configure AwsCognito:DefaultRedirectUri before third-party authentication can be used.");
        }

        return resolvedRedirectUri;
    }

    private static string[] ResolveScopes(AwsCognitoSettings settings)
    {
        return settings.OAuthScopes is { Length: > 0 }
            ? settings.OAuthScopes
            : ["openid", "email", "profile"];
    }

    private static string GetCognitoIdentityProvider(ThirdPartyAuthProvider provider)
    {
        return provider switch
        {
            ThirdPartyAuthProvider.Google => "Google",
            ThirdPartyAuthProvider.Apple => "SignInWithApple",
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported third-party authentication provider")
        };
    }

    private static string BuildQueryString(Dictionary<string, string> queryParameters)
    {
        return string.Join(
            '&',
            queryParameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
    }

    private static Dictionary<string, string> ReadJwtPayloadClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        var bytes = Convert.FromBase64String(payload);
        using var document = JsonDocument.Parse(bytes);

        return document.RootElement
            .EnumerateObject()
            .Where(property => property.Value.ValueKind == JsonValueKind.String)
            .ToDictionary(
                property => property.Name,
                property => property.Value.GetString() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string GetClaimValue(Dictionary<string, string> claims, string claimName)
    {
        return claims.TryGetValue(claimName, out var value)
            ? value
            : string.Empty;
    }

    private sealed class CognitoTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
