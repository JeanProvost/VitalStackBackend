using Backend.Core.Entities.Users;
using Backend.Core.Interfaces.IRepository;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace Backend.Infrastructure.Repository;

public class UserRepository(ApplicationDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return DbSet.AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
}
