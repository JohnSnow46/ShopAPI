using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Auth;

public class LoginUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
{
    public async Task<Result<AuthResponseDto>> ExecuteAsync(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email);
        if (user is null || !passwordHasher.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var token = jwtService.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            token,
            new UserDto(user.Id, user.Email, user.Role.ToString())));
    }
}
