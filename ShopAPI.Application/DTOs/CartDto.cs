namespace ShopAPI.Application.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal ProductPrice,
    int Quantity);

public record AddToCartDto(
    Guid ProductId,
    int Quantity);
