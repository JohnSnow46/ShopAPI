using ShopAPI.Domain.Enums;

namespace ShopAPI.Application.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);

public record OrderDto(
    Guid Id,
    Guid UserId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemDto> Items);

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity);

public record CreateOrderDto(
    List<CreateOrderItemDto> Items);
