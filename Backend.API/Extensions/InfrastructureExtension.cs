namespace Backend.API.Extensions
{
    public static class InfrastructureExtension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, bool IsDevelopment)
        {
            var configuredConnectionString = config.GetConnectionString("DefaultConnection")
                ?? config["Database:ConnectionString"];

            //services.AddDbContextPool
        }
    }
}
