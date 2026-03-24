using Moq;
using ShopAPI.Application.UseCases.Orders;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Tests.Unit.UseCases;

public class UpdateOrderStatusUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly UpdateOrderStatusUseCase _sut;

    public UpdateOrderStatusUseCaseTests()
    {
        _sut = new UpdateOrderStatusUseCase(_orderRepo.Object);
    }

    [Fact]
    public async Task Should_ReturnSuccess_WhenValidStatusTransition()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = []
        };

        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _orderRepo.Setup(r => r.UpdateStatusAsync(orderId, OrderStatus.Confirmed)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(orderId, OrderStatus.Confirmed);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, result.Value!.Status);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenInvalidStatusTransition()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Delivered,
            TotalAmount = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = []
        };

        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        var result = await _sut.ExecuteAsync(orderId, OrderStatus.Pending);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot transition", result.Error);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();

        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var result = await _sut.ExecuteAsync(orderId, OrderStatus.Confirmed);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
