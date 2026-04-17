using Backend.Core.DTOs;
using Backend.Core.Entities.Users;

namespace Backend.Core.Interfaces.IServices;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
