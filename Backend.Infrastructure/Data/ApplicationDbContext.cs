using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Core.Entities.Supplement;
using Backend.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.IdentityId).IsUnique();

                entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
                entity.Property(x => x.IdentityId).HasMaxLength(128).IsRequired();
                entity.Property(x => x.AuthProvider).HasMaxLength(50).IsRequired();

                entity.Property(x => x.PasswordHash).HasMaxLength(512);
                entity.Property(x => x.PasswordSalt).HasMaxLength(256);
            });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Supplement> Supplements { get; set; }
    }
}
