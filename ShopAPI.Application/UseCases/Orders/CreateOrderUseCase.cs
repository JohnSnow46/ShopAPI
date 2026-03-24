using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Orders;

public class CreateOrderUseCase(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository)
{
    public async Task<Result<OrderDto>> ExecuteAsync(Guid userId)
    {
        var cartItems = (await cartRepository.GetByUserIdAsync(userId)).ToList();
        if (cartItems.Count == 0)
            return Result<OrderDto>.Failure("Cart is empty.");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await productRepository.GetByIdAsync(cartItem.ProductId);
            if (product is null)
                return Result<OrderDto>.Failure($"Product {cartItem.ProductId} not found.");

            if (!product.IsActive)
                return Result<OrderDto>.Failure($"Product '{product.Name}' is not available.");

            if (product.StockQuantity < cartItem.Quantity)
                return Result<OrderDto>.Failure(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, requested: {cartItem.Quantity}.");

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price
            });

            totalAmount += product.Price * cartItem.Quantity;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        var created = await orderRepository.AddAsync(order);

        foreach (var item in orderItems)
            await productRepository.UpdateStockAsync(item.ProductId, -item.Quantity);

        await cartRepository.ClearCartAsync(userId);

        return Result<OrderDto>.Success(ToDto(created));
    }

    private static OrderDto ToDto(Order order) => new(
        order.Id,
        order.UserId,
        order.Status,
        order.TotalAmount,
        order.CreatedAt,
        order.UpdatedAt,
        order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Quantity, i.UnitPrice)).ToList());
}
