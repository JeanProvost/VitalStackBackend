using Amazon;
using Backend.Core.Configuration;
using Amazon.Runtime;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Extensions.Configuration.SystemsManager;

namespace Backend.API.Extensions
{
    public static class ConfigurationExtensions
    {
        public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
        {
            // Load additional config sources FIRST so values are available below
            var awsOptions = builder.Configuration.GetAWSOptions();
            var accessKey = builder.Configuration["AWS:AccessKeyId"];
            var secretAccessKey = builder.Configuration["AWS:SecretAccessKey"];

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretAccessKey))
            {
                awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretAccessKey);
                awsOptions.Region = RegionEndpoint.CACentral1;
            }

            builder.Services.AddDefaultAWSOptions(awsOptions);

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }
            else
            {
                var env = builder.Environment.EnvironmentName;

                builder.Configuration.AddSystemsManager(config =>
                {
                    config.Path = $"/vitalstack-backend/{env}";
                    config.ReloadAfter = TimeSpan.FromMinutes(15);
                    config.Optional = true;
                    config.AwsOptions = awsOptions;
                });
            }

            builder.Services.AddOptions<DataBaseSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    settings.ConnectionString =
                        configuration.GetConnectionString("DefaultConnection")
                        ?? configuration[$"{DataBaseSettings.SectionName}:ConnectionString"]
                        ?? string.Empty;
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<AwsCognitoSettings>()
                .BindConfiguration(AwsCognitoSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return builder;
        }
    }
}
