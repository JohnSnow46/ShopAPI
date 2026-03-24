using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Orders;

public class GetOrdersUseCase(IOrderRepository orderRepository)
{
    public async Task<Result<List<OrderDto>>> ExecuteAsync(Guid userId, bool isAdmin)
    {
        var orders = isAdmin
            ? await orderRepository.GetAllAsync()
            : await orderRepository.GetByUserIdAsync(userId);

        var dtos = orders
            .Select(o => new OrderDto(
                o.Id,
                o.UserId,
                o.Status,
                o.TotalAmount,
                o.CreatedAt,
                o.UpdatedAt,
                o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Quantity, i.UnitPrice)).ToList()))
            .ToList();

        return Result<List<OrderDto>>.Success(dtos);
    }
}
