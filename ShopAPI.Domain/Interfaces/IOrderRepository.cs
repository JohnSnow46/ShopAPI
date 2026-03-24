using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;

namespace ShopAPI.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> AddAsync(Order order);
    Task UpdateStatusAsync(Guid id, OrderStatus status);
}
