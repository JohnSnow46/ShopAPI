using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Orders;

public class UpdateOrderStatusUseCase(IOrderRepository orderRepository)
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        [OrderStatus.Pending]   = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipped,   OrderStatus.Cancelled],
        [OrderStatus.Shipped]   = [OrderStatus.Delivered, OrderStatus.Cancelled],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = [],
    };

    public async Task<Result<OrderDto>> ExecuteAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return Result<OrderDto>.Failure("Order not found.");

        if (!ValidTransitions[order.Status].Contains(newStatus))
            return Result<OrderDto>.Failure(
                $"Cannot transition order from '{order.Status}' to '{newStatus}'.");

        await orderRepository.UpdateStatusAsync(orderId, newStatus);

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        return Result<OrderDto>.Success(new OrderDto(
            order.Id,
            order.UserId,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Quantity, i.UnitPrice)).ToList()));
    }
}
