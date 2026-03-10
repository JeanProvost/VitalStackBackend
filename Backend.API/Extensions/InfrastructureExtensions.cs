namespace Backend.API.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, bool IsDevelopment)
        {
            var configuredConnectionString = config.GetConnectionString("DefaultConnection")
                ?? config["Database:ConnectionString"];

            //services.AddDbContextPool
        }
    }
}
