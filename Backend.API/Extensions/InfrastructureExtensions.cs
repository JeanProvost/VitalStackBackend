using Backend.Core.Configuration;
using Backend.Infrastructure.Data;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, bool isDevelopment)
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? config[$"{DataBaseSettings.SectionName}:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured.");
            }

            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.SetPostgresVersion(17, 0);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                });

                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            services.AddHealthChecks();
            ////services.AddHealthChecks()
            //    .AddPostgres(connectionString, name: "PostgreSQL", tags: new[] { "db", "sql", "postgres" });
            //services.AddHybridCache();

            //AWS SDK
            services.AddAWSService<IAmazonCognitoIdentityProvider>();
            //services.AddAWSService<IAmazonSimpleEmailService>();

            return services;
        }
    }
}
