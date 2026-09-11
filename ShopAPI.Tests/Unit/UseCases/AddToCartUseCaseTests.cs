using Moq;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Cart;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Tests.Unit.UseCases;

public class AddToCartUseCaseTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly AddToCartUseCase _sut;

    public AddToCartUseCaseTests()
    {
        _sut = new AddToCartUseCase(_cartRepo.Object, _productRepo.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_ReturnFailure_WhenQuantityIsNotPositive(int quantity)
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 10m, StockQuantity = 5, IsActive = true });

        var result = await _sut.ExecuteAsync(userId, new AddToCartDto(productId, quantity));

        Assert.False(result.IsSuccess);
        Assert.Contains("quantity", result.Error, StringComparison.OrdinalIgnoreCase);
        _cartRepo.Verify(r => r.AddOrUpdateItemAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenQuantityIsPositive()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 10m, StockQuantity = 5, IsActive = true });

        _cartRepo.Setup(r => r.AddOrUpdateItemAsync(It.IsAny<CartItem>()))
            .ReturnsAsync((CartItem item) => item);

        var result = await _sut.ExecuteAsync(userId, new AddToCartDto(productId, 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Quantity);
    }
}
