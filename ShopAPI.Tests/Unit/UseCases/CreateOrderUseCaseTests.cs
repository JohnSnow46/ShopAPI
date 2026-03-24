using Moq;
using ShopAPI.Application.UseCases.Orders;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Tests.Unit.UseCases;

public class CreateOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly CreateOrderUseCase _sut;

    public CreateOrderUseCaseTests()
    {
        _sut = new CreateOrderUseCase(_orderRepo.Object, _cartRepo.Object, _productRepo.Object);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenCartHasItemsAndStockAvailable()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([new CartItem { Id = Guid.NewGuid(), UserId = userId, ProductId = productId, Quantity = 2 }]);

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 50m, StockQuantity = 10, IsActive = true });

        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        var result = await _sut.ExecuteAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100m, result.Value.Order.TotalAmount);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenCartIsEmpty()
    {
        var userId = Guid.NewGuid();

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([]);

        var result = await _sut.ExecuteAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenProductOutOfStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([new CartItem { UserId = userId, ProductId = productId, Quantity = 5 }]);

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 10m, StockQuantity = 2, IsActive = true });

        var result = await _sut.ExecuteAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Contains("stock", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_ReduceStockQuantity_AfterSuccessfulOrder()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        const int orderedQty = 3;

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([new CartItem { UserId = userId, ProductId = productId, Quantity = orderedQty }]);

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 10m, StockQuantity = 10, IsActive = true });

        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        await _sut.ExecuteAsync(userId);

        _productRepo.Verify(r => r.UpdateStockAsync(productId, -orderedQty), Times.Once);
    }

    [Fact]
    public async Task Should_ClearCart_AfterSuccessfulOrder()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _cartRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([new CartItem { UserId = userId, ProductId = productId, Quantity = 1 }]);

        _productRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, Name = "Test", Price = 10m, StockQuantity = 5, IsActive = true });

        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        await _sut.ExecuteAsync(userId);

        _cartRepo.Verify(r => r.ClearCartAsync(userId), Times.Once);
    }
}
