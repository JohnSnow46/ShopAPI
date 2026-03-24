using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Auth;

public class RegisterUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public async Task<Result<UserDto>> ExecuteAsync(RegisterDto dto)
    {
        var existing = await userRepository.GetByEmailAsync(dto.Email);
        if (existing is not null)
            return Result<UserDto>.Failure("Email is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = passwordHasher.Hash(dto.Password),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        var created = await userRepository.AddAsync(user);

        return Result<UserDto>.Success(new UserDto(
            created.Id,
            created.Email,
            created.Role.ToString()));
    }
}
