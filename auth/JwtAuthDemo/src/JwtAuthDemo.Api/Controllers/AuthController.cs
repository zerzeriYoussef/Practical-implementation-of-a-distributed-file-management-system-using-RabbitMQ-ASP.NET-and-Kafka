using System.Security.Claims;
using JwtAuthDemo.Application.Auth;
using JwtAuthDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthenticationService _authentication;

    public AuthController(AuthenticationService authentication)
    {
        _authentication = authentication;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authentication.RegisterAsync(request, cancellationToken);

        return response is null
            ? BadRequest(new { message = "Invalid registration request or user already exists." })
            : Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authentication.LoginAsync(request, cancellationToken);

        return response is null
            ? Unauthorized()
            : Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(TokenRefreshRequest request, CancellationToken cancellationToken)
    {
        var response = await _authentication.RefreshTokenAsync(request, cancellationToken);

        return response is null
            ? Unauthorized()
            : Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult GetCurrentUser()
    {
        var user = new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            FirstName = User.FindFirstValue(ClaimTypes.GivenName),
            LastName = User.FindFirstValue(ClaimTypes.Surname)
        };

        return Ok(user);
    }
}
