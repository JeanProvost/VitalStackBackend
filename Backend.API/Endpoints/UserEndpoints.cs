using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IServices;
using System.ComponentModel.DataAnnotations;

namespace Backend.API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", async (
            RegisterRequest request,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = Validate(request);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors);
            }

            await userService.RegisterAsync(request, cancellationToken);

            return Results.Ok("User successfully registered");
        });

        group.MapPost("/login", async (
            LoginDto request,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = Validate(request);
            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var loginResponse = await userService.LoginAsync(request, cancellationToken);

                return Results.Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .AllowAnonymous();

        return group;
    }

    private static Dictionary<string, string[]> Validate<TRequest>(TRequest request)
        where TRequest : class
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        if (Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            return [];
        }

        return validationResults
            .SelectMany(result =>
            {
                return result.MemberNames
                    .DefaultIfEmpty(string.Empty)
                    .Select(member => new
                {
                    Member = member,
                    Error = result.ErrorMessage ?? "The request is invalid."
                });
            })
            .GroupBy(error => error.Member)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Error).ToArray());
    }
}
