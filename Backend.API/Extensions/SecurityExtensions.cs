using Backend.Core.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.NetworkInformation;

namespace Backend.API.Extensions
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AwsCognitoSettings>(configuration.GetSection("AwsCognitoSettings"));
            services.Configure<CorsSettings>(configuration.GetSection("CorsSetting"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var cognitoSettings = serviceProvider.GetRequiredService<IOptions<AwsCognitoSettings>>().Value;

                    options.Authority = $"https://cognito-idp.{cognitoSettings.CognitoRegion}.amazonaws.com/{cognitoSettings.UserPoolId}";
                    options.Audience = cognitoSettings.ClientId;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = $"https://cognito-idp.{cognitoSettings.CognitoRegion}.amazonaws.com/{cognitoSettings.UserPoolId}",
                        ValidAudience = cognitoSettings.ClientId,
                        RoleClaimType = "cognito:groups",
                    };
                });
            services.AddAuthorization();

            services.AddCors(options =>
            {
                var buildService = services.BuildServiceProvider();
                var corsSettings = buildService.GetRequiredService<IOptions<CorsSettings>>().Value;

                options.AddPolicy(CorsSettings.PolicyName, policy =>
                {
                    if (corsSettings.AllowOrigins is { Length: > 0 })
                    {
                        policy.WithOrigins(corsSettings.AllowOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                    else
                    {
                        policy.DisallowCredentials();
                    }
                });
            });
            return services;
        }
    }
}
