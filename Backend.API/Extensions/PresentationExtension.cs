using Scalar.AspNetCore;

namespace Backend.API.Extensions
{
    public static class PresentationExtension
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddProblemDetails();

            services.AddOpenApi("v1", options =>
            {
                options.AddDocumentTransformer((document, context, CancellationToken) =>
                {
                    document.Info = new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "VitalStackBackend",
                        Version = "v1",
                        Description = "Back-end service for VS"
                    };
                    return Task.CompletedTask;
                });
            });
            services.AddEndpointsApiExplorer();
            return services;
        }

        public static WebApplication UsePresentation(this WebApplication app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/scalar", options =>
            {
                options.WithTitle("vitalstack-backend")
                    .WithClassicLayout()
                    .WithOpenApiRoutePattern("/openapi/v1.json");
            });

            return app;
        }
    }
}
