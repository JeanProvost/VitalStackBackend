using Amazon.CognitoIdentityProvider;
using Backend.API.Extensions;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

builder.AddConfiguration();
builder.Services.AddPresentation();
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    Console.WriteLine("Applying migrations...");
    //var identityDb = services.GetRequiredService<KarbonX.Infrastructure.I>
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var test = db.Database.GetDbConnection().ConnectionString;

    if (string.IsNullOrEmpty(test))
    {
        Console.WriteLine("ConnectionString Empty...");
    }
    Console.WriteLine(db.Database.GetDbConnection().ConnectionString);

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
}

app.MapControllers();
app.UseMiddleware();
await app.RunAsync();

public partial class Program { }
