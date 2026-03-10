using System.Runtime.CompilerServices;

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
                        Description = "Backend service for VS"
                    };
                    return Task.CompletedTask;
                });
            });
            services.AddEndpointsApiExplorer();
            return services;
        }
    }
}
