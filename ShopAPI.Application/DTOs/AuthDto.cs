namespace ShopAPI.Application.DTOs;

public record RegisterDto(
    string Email,
    string Password);

public record LoginDto(
    string Email,
    string Password);

public record UserDto(
    Guid Id,
    string Email,
    string Role);

public record AuthResponseDto(
    string Token,
    UserDto User);
