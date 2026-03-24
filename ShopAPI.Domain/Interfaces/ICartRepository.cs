using ShopAPI.Domain.Entities;

namespace ShopAPI.Domain.Interfaces;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId);
    Task<CartItem> AddOrUpdateItemAsync(CartItem item);
    Task RemoveItemAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
}
