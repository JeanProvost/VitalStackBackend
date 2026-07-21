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
            ConfigureSupplementCatalog(modelBuilder);
            ConfigureUserStackEntries(modelBuilder);
            ConfigureIntakeLogs(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SupplementIngredient> SupplementIngredients { get; set; }
        public DbSet<SupplementProduct> SupplementProducts { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<UserStackEntry> UserStackEntries { get; set; }
        public DbSet<IntakeLog> IntakeLogs { get; set; }

        private static void ConfigureSupplementCatalog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplementIngredient>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.CanonicalName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Category).HasMaxLength(100).IsRequired();
                b.Property(x => x.Aliases).HasColumnType("jsonb");
                b.HasIndex(x => x.CanonicalName).IsUnique();
            });

            modelBuilder.Entity<SupplementProduct>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.DsldId).HasMaxLength(50).IsRequired();
                b.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
                b.Property(x => x.BrandName).HasMaxLength(200);
                b.Property(x => x.Form).HasMaxLength(100).IsRequired();
                b.Property(x => x.Metadata).HasColumnType("jsonb");
                b.HasIndex(x => x.DsldId).IsUnique();
            });

            modelBuilder.Entity<ProductIngredient>(b =>
            {
                b.HasKey(x => new { x.ProductId, x.IngredientId });
                b.Property(x => x.DosageAmount).IsRequired();
                b.Property(x => x.DosageUnit).HasMaxLength(50).IsRequired();
                b.HasOne(x => x.Product)
                    .WithMany(p => p.ActiveIngredients)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Ingredient)
                    .WithMany(i => i.ProductLinks)
                    .HasForeignKey(x => x.IngredientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

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

                builder.Property(x => x.ServingMultiplier)
                    .HasPrecision(5, 2);

                builder.HasIndex(x => new { x.UserId, x.IsActive, x.IntendedTime });

                builder.HasOne(x => x.SupplementProduct)
                    .WithMany()
                    .HasForeignKey(x => x.SupplementProductId)
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
