using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs.Auth;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        await _authService.LogoutAsync(dto);

        return NoContent();
    }

    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _authService.LogoutAllAsync(userId);

        return NoContent();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }
}
