using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Cart;

public class AddToCartUseCase(ICartRepository cartRepository, IProductRepository productRepository)
{
    public async Task<Result<CartItemDto>> ExecuteAsync(Guid userId, AddToCartDto dto)
    {
        var product = await productRepository.GetByIdAsync(dto.ProductId);
        if (product is null)
            return Result<CartItemDto>.Failure("Product not found.");

        if (!product.IsActive)
            return Result<CartItemDto>.Failure("Product is not available.");

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            AddedAt = DateTime.UtcNow
        };

        var saved = await cartRepository.AddOrUpdateItemAsync(cartItem);

        return Result<CartItemDto>.Success(new CartItemDto(
            saved.Id,
            saved.ProductId,
            product.Name,
            product.Price,
            saved.Quantity));
    }
}
