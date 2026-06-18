using Backend.Core.Entities.Users.DTOs;
using Backend.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : Controller
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await userService.RegisterAsync(request, cancellationToken);

        return Ok("User successfully registered");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var loginResponse = await userService.LoginAsync(request, cancellationToken);

            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("third-party/{provider}/authorize-url")]
    [AllowAnonymous]
    public IActionResult GetThirdPartyAuthorizationUrl(
        ThirdPartyAuthProvider provider,
        [FromQuery] string? redirectUri,
        [FromQuery] string? state,
        [FromQuery] string? codeChallenge)
    {
        try
        {
            var response = userService.GetThirdPartyAuthorizationUrl(
                provider,
                redirectUri,
                state,
                codeChallenge);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("third-party/{provider}/login")]
    [AllowAnonymous]
    public async Task<IActionResult> ThirdPartyLogin(
        ThirdPartyAuthProvider provider,
        [FromBody] ThirdPartyLoginDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var loginResponse = await userService.ThirdPartyLoginAsync(
                provider,
                request,
                cancellationToken);

            return Ok(loginResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
