using Backend.Core.DTOs;
using Backend.Core.Interfaces.IServices;
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
        var user = await userService.RegisterAsync(request, cancellationToken);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IdentityId
        });
    }
}
