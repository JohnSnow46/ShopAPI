using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Cart;

public class GetCartUseCase(ICartRepository cartRepository, IProductRepository productRepository)
{
    public async Task<Result<List<CartItemDto>>> ExecuteAsync(Guid userId)
    {
        var cartItems = (await cartRepository.GetByUserIdAsync(userId)).ToList();

        var dtos = new List<CartItemDto>(cartItems.Count);
        foreach (var item in cartItems)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            if (product is null) continue;

            dtos.Add(new CartItemDto(
                item.Id,
                item.ProductId,
                product.Name,
                product.Price,
                item.Quantity));
        }

        return Result<List<CartItemDto>>.Success(dtos);
    }
}
