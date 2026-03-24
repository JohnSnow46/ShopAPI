using Microsoft.EntityFrameworkCore;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;
using ShopAPI.Domain.Interfaces;
using ShopAPI.Infrastructure.Data;

namespace ShopAPI.Infrastructure.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id) =>
        await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId) =>
        await db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetAllAsync() =>
        await db.Orders
            .Include(o => o.Items)
            .ToListAsync();

    public async Task<Order> AddAsync(Order order)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    public async Task UpdateStatusAsync(Guid id, OrderStatus status)
    {
        await db.Orders
            .Where(o => o.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, status)
                .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));
    }
}
