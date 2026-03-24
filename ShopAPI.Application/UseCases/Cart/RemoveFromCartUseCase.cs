using ShopAPI.Application.Common;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Cart;

public class RemoveFromCartUseCase(ICartRepository cartRepository)
{
    public async Task<Result<bool>> ExecuteAsync(Guid userId, Guid productId)
    {
        await cartRepository.RemoveItemAsync(userId, productId);
        return Result<bool>.Success(true);
    }
}
