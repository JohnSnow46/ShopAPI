using Microsoft.AspNetCore.Mvc;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Auth;

namespace ShopAPI.API.Controllers;

/// <summary>
/// Handles user authentication: registration and login.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(RegisterUseCase registerUseCase, LoginUseCase loginUseCase) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">Registration payload (email, password, full name).</param>
    /// <returns>200 with JWT token and user info on success; 400 on validation error or duplicate email.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await registerUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token.
    /// </summary>
    /// <param name="dto">Login credentials (email and password).</param>
    /// <returns>200 with JWT token and user info on success; 401 on invalid credentials.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await loginUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });
        return Ok(result.Value);
    }
}
