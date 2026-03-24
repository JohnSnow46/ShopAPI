using ShopAPI.Domain.Entities;

namespace ShopAPI.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(
        string? category = null,
        int page = 1,
        int pageSize = 20);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
    Task UpdateStockAsync(Guid id, int quantity);
}
