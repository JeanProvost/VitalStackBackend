using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.DTOs;

namespace Backend.Core.Interfaces.IServices;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
