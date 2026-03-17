using Backend.Core.Configuration;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, bool isDevelopment)
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? config[$"{DataBaseSettings.SectionName}:ConnectionString"];

            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.SetPostgresVersion(17, 0));

                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            return services;
        }
    }
}
