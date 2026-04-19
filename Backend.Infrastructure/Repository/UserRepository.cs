using Backend.Core.Entities.Users;
using Backend.Core.Interfaces.IRepository;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repository;

public class UserRepository(ApplicationDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
}
