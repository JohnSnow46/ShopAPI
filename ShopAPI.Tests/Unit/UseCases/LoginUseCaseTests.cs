using Moq;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.Interfaces;
using ShopAPI.Application.UseCases.Auth;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Tests.Unit.UseCases;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _sut = new LoginUseCase(_userRepo.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Should_ReturnToken_WhenCredentialsValid()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", PasswordHash = "hash", Role = UserRole.Customer };
        var dto = new LoginDto("user@test.com", "password");

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");

        var result = await _sut.ExecuteAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Value!.Token);
        Assert.Equal(user.Email, result.Value.User.Email);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenEmailNotFound()
    {
        var dto = new LoginDto("nobody@test.com", "password");

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

        var result = await _sut.ExecuteAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid", result.Error);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenPasswordIncorrect()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", PasswordHash = "hash", Role = UserRole.Customer };
        var dto = new LoginDto("user@test.com", "wrong-password");

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        var result = await _sut.ExecuteAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid", result.Error);
    }
}
