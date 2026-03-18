using Backend.Core.Configuration;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace Backend.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.MapOpenApi();

                app.MapScalarApiReference("/scalar", options =>
                {
                    options.WithTitle("VitalStack-Backend")
                        .WithClassicLayout()
                        .WithOpenApiRoutePattern("/openapi/v1.json");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            //app.UseSerilogRequestLogging();
            //TODO: Serilog configuration
            app.UseRouting();
            app.UseCors(CorsSettings.PolicyName);

            app.UseAuthorization();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");
            //TODO: Hangfire config

            return app;
        }

        public static void Run(this WebApplication app, string[] args)
        {
            ApplyMigrations(app.Services);
        } 

        private static void ApplyMigrations(IServiceProvider services)
        {
            Console.WriteLine("Applying migrations...");
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations complete");
        }

    }
}
