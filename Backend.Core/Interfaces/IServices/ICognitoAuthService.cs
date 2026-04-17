namespace Backend.Core.Interfaces.IServices;

public interface ICognitoAuthService
{
    Task<string> SignUpAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
}
