using Backend.Core.Entities.IntakeLogs;
using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Users;
using Backend.Core.Entities.Users.Persistence;
using Backend.Core.Entities.UserStackEntries;
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

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            ConfigureUserStackEntries(modelBuilder);
            ConfigureIntakeLogs(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Supplement> Supplements { get; set; }
        public DbSet<UserStackEntry> UserStackEntries { get; set; }
        public DbSet<IntakeLog> IntakeLogs { get; set; }

        private static void ConfigureUserStackEntries(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserStackEntry>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id)
                    .ValueGeneratedOnAdd();

                builder.Property(x => x.UserId)
                    .IsRequired();

                builder.Property(x => x.IntendedTime)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                builder.Property(x => x.ContextualInstruction)
                    .HasMaxLength(500);

                builder.HasIndex(x => new { x.UserId, x.IsActive, x.IntendedTime });

                builder.HasOne(x => x.MasterSupplement)
                    .WithMany()
                    .HasForeignKey(x => x.MasterSupplementId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureIntakeLogs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntakeLog>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id)
                    .ValueGeneratedOnAdd();

                builder.Property(x => x.UserId)
                    .IsRequired();

                builder.Property(x => x.IntendedTime)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                builder.Property(x => x.SupplementName)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.Property(x => x.Dosage)
                    .HasMaxLength(100);

                builder.Property(x => x.ContextualInstruction)
                    .HasMaxLength(500);

                builder.HasIndex(x => new { x.UserId, x.TakenAtUtc });
                builder.HasIndex(x => new { x.UserId, x.UserStackEntryId, x.TakenAtUtc });

                builder.HasOne(x => x.UserStackEntry)
                    .WithMany()
                    .HasForeignKey(x => x.UserStackEntryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
