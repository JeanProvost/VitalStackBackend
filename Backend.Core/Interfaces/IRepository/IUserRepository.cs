using Backend.Core.Entities.Users;

namespace Backend.Core.Interfaces.IRepository;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
}
