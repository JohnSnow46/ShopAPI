using Microsoft.AspNetCore.Mvc;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Auth;

namespace ShopAPI.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(RegisterUseCase registerUseCase, LoginUseCase loginUseCase) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await registerUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await loginUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });
        return Ok(result.Value);
    }
}
