using Microsoft.EntityFrameworkCore;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;
using ShopAPI.Infrastructure.Data;

namespace ShopAPI.Infrastructure.Repositories;

public class CartRepository(AppDbContext db) : ICartRepository
{
    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId) =>
        await db.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

    public async Task<CartItem> AddOrUpdateItemAsync(CartItem item)
    {
        var existing = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == item.UserId && c.ProductId == item.ProductId);

        if (existing is null)
        {
            db.CartItems.Add(item);
        }
        else
        {
            existing.Quantity = item.Quantity;
            existing.AddedAt = item.AddedAt;
        }

        await db.SaveChangesAsync();
        return existing ?? item;
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        await db.CartItems
            .Where(c => c.UserId == userId && c.ProductId == productId)
            .ExecuteDeleteAsync();
    }

    public async Task ClearCartAsync(Guid userId)
    {
        await db.CartItems
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync();
    }
}
