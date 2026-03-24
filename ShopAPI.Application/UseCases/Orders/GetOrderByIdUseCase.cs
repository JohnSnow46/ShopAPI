using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Orders;

public class GetOrderByIdUseCase(IOrderRepository orderRepository)
{
    public async Task<Result<OrderDto>> ExecuteAsync(Guid orderId, Guid userId, bool isAdmin)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return Result<OrderDto>.Failure("Order not found.");

        if (!isAdmin && order.UserId != userId)
            return Result<OrderDto>.Failure("Order not found.");

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
