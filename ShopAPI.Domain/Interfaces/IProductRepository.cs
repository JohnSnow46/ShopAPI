using ShopAPI.Domain.Common;
using ShopAPI.Domain.Entities;

namespace ShopAPI.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<PagedResult<Product>> GetAllAsync(
        string? category = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
    Task UpdateStockAsync(Guid id, int quantity);
}
