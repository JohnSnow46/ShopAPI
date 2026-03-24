using ShopAPI.Application.Common;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Cart;

public class ClearCartUseCase(ICartRepository cartRepository)
{
    public async Task<Result<bool>> ExecuteAsync(Guid userId)
    {
        await cartRepository.ClearCartAsync(userId);
        return Result<bool>.Success(true);
    }
}
